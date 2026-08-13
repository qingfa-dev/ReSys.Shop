using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Services;

using Pgvector;

namespace Module.Catalog.Features.Storefront.Products.Images.Search;

/// <summary>
/// Defines the use case for searching products by image similarity.
/// </summary>
public static partial class SearchByImage
{
    public sealed record Command(Request Request) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        IInferenceClient inferenceClient,
        IVectorSearchService vectorSearchService)
        : IPagedQueryHandler<Command, Response>
    {
        private const string DefaultModel = VariantImageConstant.Defaults.DefaultEmbeddingModel;
        private const long MaxFileSize = 10_485_760; // 10 MB

        /// <summary>
        /// Performs a visual similarity search by encoding an uploaded image into an embedding vector
        /// and querying the nearest neighbors in pgvector space.
        /// </summary>
        // Contract: pre=command!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var image = command.Request.Image;

            // Validate: Reject empty, oversized, and non-image files
            if (image is null || image.Length == 0)
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 1, totalCount: 0);

            if (image.Length > MaxFileSize)
                return SearchByImageResult.Errors.FileTooLarge;

            if (string.IsNullOrEmpty(image.ContentType) || !image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return SearchByImageResult.Errors.InvalidContentType;

            // Transform: Read image bytes into memory for inference
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);
            var imageBytes = ms.ToArray();

            var modelName = command.Request.Model ?? DefaultModel;
            var topK = command.Request.TopK > 0 ? command.Request.TopK : 20;

            // Call: Generate embedding vector from uploaded image via inference service
            var inferenceResult = await inferenceClient.CreateEmbeddingFromBytesAsync(
                imageBytes, image.ContentType, modelName, cancellationToken);

            if (inferenceResult.IsFailure)
                return inferenceResult.Errors;

            var embedding = inferenceResult.Value;
            var queryVector = new Vector(embedding.Vector.ToArray());

            // Query: Find nearest neighbors in vector space with scores. Fetch a superset of
            // variants so that de-duplicating to one item per product still yields TopK products.
            var similarResults = await vectorSearchService.FindSimilarWithScoresAsync(
                queryVector, modelName, topK * 2, cancellationToken: cancellationToken);

            if (similarResults.Count == 0)
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: topK, totalCount: 0);

            // Map: Resolve which product each matching variant belongs to.
            var variantIds = similarResults.Select(r => r.VariantId).ToList();
            var productByVariant = await dbContext.Set<Variant>()
                .AsNoTracking()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new { v.Id, v.ProductId })
                .ToDictionaryAsync(v => v.Id, v => v.ProductId, cancellationToken);

            // Aggregate: One result per product, ranked by the best-scoring matching variant.
            var scoresByProduct = new Dictionary<Guid, double>();
            foreach (var result in similarResults)
            {
                if (!productByVariant.TryGetValue(result.VariantId, out var productId))
                    continue;

                scoresByProduct.TryGetValue(productId, out var currentScore);
                if (result.Score > currentScore)
                    scoresByProduct[productId] = result.Score;
            }

            var rankedProducts = scoresByProduct
                .OrderByDescending(kvp => kvp.Value)
                .Take(topK)
                .ToList();

            // Load: Fetch the full product graph required by MapToStoreListItem, then map
            // results preserving the similarity ranking.
            var productIds = rankedProducts.Select(kvp => kvp.Key).ToList();
            var products = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue!)
                            .ThenInclude(o => o.OptionType!)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var productsById = products.ToDictionary(p => p.Id);
            var items = rankedProducts
                .Where(kvp => productsById.ContainsKey(kvp.Key))
                .Select(kvp =>
                {
                    var item = productsById[kvp.Key].MapToStoreListItem<Response>();
                    return item with { SimilarityScore = kvp.Value };
                })
                .ToList();

            return PagedResult<Response>.Create(items, page: 1, pageSize: topK, totalCount: items.Count);
        }
    }
}