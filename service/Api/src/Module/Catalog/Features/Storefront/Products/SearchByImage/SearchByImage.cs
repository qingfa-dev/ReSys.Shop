using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

using Pgvector;

namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

/// <summary>
/// Defines the use case for searching products by image similarity.
/// </summary>
public static partial class SearchByImage
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IInferenceClient inferenceClient)
        : ICommandHandler<Command, Response>
    {
        private const string DefaultModel = VariantImageConstant.Defaults.DefaultEmbeddingModel;

        /// <summary>
        /// Performs a visual similarity search by encoding an uploaded image into an embedding vector
        /// and querying the nearest neighbors in pgvector space.
        /// </summary>
        /// <param name="command">The command containing the uploaded image file.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the list of visually similar product variants.</returns>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var image = command.Request.Image;

            // Validate: Reject empty, oversized, and non-image files
            if (image is null || image.Length == 0)
                return new Response();

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

            // Query: Find nearest neighbors in pgvector space using cosine distance
            var similarVariants = await dbContext.Set<Variant>()
                .FromSqlRaw(@"
                    SELECT DISTINCT ON (v.id) v.*
                    FROM catalog.variants v
                    INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
                    INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
                    WHERE v.is_deleted = false
                      AND vi.type = 'Default'
                      AND ie.model_name = {1}
                    ORDER BY v.id, ie.vector <=> {0}::vector
                    LIMIT {2}",
                    queryVector, modelName, topK)
                .Include(x => x.Product)
                .Include(x => x.VariantImages)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Build search result items with variant and image URLs
            var items = similarVariants.Select(MapToItem).ToList();

            return new Response { Items = items };
        }
    }

    private static SearchResultItem MapToItem(Variant v)
    {
        var primaryImage = v.VariantImages.FirstOrDefault();
        return new SearchResultItem
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