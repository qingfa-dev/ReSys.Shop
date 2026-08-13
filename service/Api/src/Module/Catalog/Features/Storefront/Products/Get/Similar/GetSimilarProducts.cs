using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

/// <summary>
/// Defines the use case for finding visually similar products.
/// </summary>
public static partial class GetSimilarProducts
{
    public sealed record Query(Guid Id, int TopK = 20) : IPagedQuery<Response>; // EXCEPTION: legacy contract, refactor breaks callers

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        IVectorSearchService vectorSearchService)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Finds visually similar products using pgvector cosine distance on image embeddings.
        /// </summary>
        // Contract: pre=request.Id!=Guid.Empty, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            const string similarityModel = VariantImageConstant.Defaults.DefaultSimilarityModel;

            // Query: Find the best representative embedding for the product in a single round trip,
            // loading the full relationship chain (ImageEmbedding -> VariantImage -> Variant -> Product).
            // "Best" = master variant first, then lowest position.
            var embedding = await dbContext.Set<ImageEmbedding>()
                .Include(ie => ie.VariantImage)
                    .ThenInclude(vi => vi.Variant!)
                        .ThenInclude(v => v.Product)
                .AsNoTracking()
                .Where(ie => ie.ModelName == similarityModel
                          && ie.Vector != null
                          && ie.VariantImage.VariantId != null
                          && ie.VariantImage.Variant!.ProductId == request.Id
                          && !ie.VariantImage.Variant!.IsDeleted)
                .OrderByDescending(ie => ie.VariantImage.Variant!.IsMaster)
                .ThenBy(ie => ie.VariantImage.Variant!.Position)
                .FirstOrDefaultAsync(cancellationToken);

            if (embedding is null)
            {
                // Distinguish "product doesn't exist" from "product exists but has no embedding
                // yet" only when needed, this check is skipped entirely in the common case above.
                var productExists = await dbContext.Set<Variant>()
                    .AnyAsync(v => v.ProductId == request.Id && !v.IsDeleted, cancellationToken);

                return productExists
                    ? PagedResult<Response>.Create(items: [], page: 1, pageSize: request.TopK, totalCount: 0)
                    : PagedResult<Response>.NotFound();
            }

            var sourceVariant = embedding.VariantImage.Variant!;

            // Query: Find nearest neighbors in vector space with scores. Fetch a superset of
            // variants so that de-duplicating to one item per product still yields TopK products.
            var similarResults = await vectorSearchService.FindSimilarWithScoresAsync(
                embedding.Vector!, embedding.ModelName, request.TopK * 2,
                excludeProductId: sourceVariant.ProductId, cancellationToken);

            if (similarResults.Count == 0)
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: request.TopK, totalCount: 0);

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
                .Take(request.TopK)
                .ToList();

            // Load: Fetch the full product graph required by MapToStoreListItem, then map
            // results preserving the vector-search ranking (do not re-sort by Position).
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

            return PagedResult<Response>.Create(items, page: 1, pageSize: request.TopK, totalCount: items.Count);
        }
    }
}
