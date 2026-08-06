using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
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
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 0, totalCount: 0);

            const long MaxFileSize = 10_485_760; // 10 MB
            if (image.Length > MaxFileSize)
                return SearchByImageResult.Errors.FileTooLarge;

            if (!image.ContentType.StartsWith("image/"))
                return SearchByImageResult.Errors.InvalidContentType;

            // Transform: Read image bytes into memory for inference
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);
            var imageBytes = ms.ToArray();

            var modelName = command.Request.Model ?? DefaultModel;

            // Call: Generate embedding vector from uploaded image via inference service
            var inferenceResult = await inferenceClient.CreateEmbeddingFromBytesAsync(
                imageBytes, image.ContentType, modelName, cancellationToken);

            if (inferenceResult.IsFailure)
                return inferenceResult.Errors;

            var embedding = inferenceResult.Value;
            var queryVector = new Vector(embedding.Vector.ToArray());

            var topK = command.Request.TopK > 0 ? command.Request.TopK : 20;

            // Query: Find nearest neighbors in vector space using cosine distance
            var similarVariantIds = await vectorSearchService.FindSimilarVariantIdsAsync(
                queryVector, modelName, topK, excludeProductId: null, cancellationToken);

            if (similarVariantIds.Count == 0)
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 0, totalCount: 0);

            // Load: Fetch full variant data with includes for response mapping
            var similarVariants = await dbContext.Set<Variant>()
                .Where(v => similarVariantIds.Contains(v.Id))
                .Include(x => x.Product)
                .Include(x => x.VariantImages)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Order: Preserve the similarity ranking from the vector search
            var orderedVariants = similarVariantIds
                .Select(id => similarVariants.First(v => v.Id == id))
                .ToList();

            // Map: Build search result items with variant and image URLs
            var items = orderedVariants.Select(MapToItem).ToList();

            return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
        }
    }

    private static Response MapToItem(Variant v)
    {
        var primaryImage = v.VariantImages.FirstOrDefault();
        return new Response
        {
            VariantId = v.Id,
            ProductId = v.ProductId,
            ProductName = v.Product?.Name ?? string.Empty,
            Sku = v.Sku ?? string.Empty,
            Price = v.Price ?? 0,
            ImageUrl = primaryImage?.Url
        };
    }
}