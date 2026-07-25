using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Storefront.Products.Shared.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

/// <summary>
/// Defines the use case for finding visually similar products.
/// </summary>
public static partial class GetSimilarProducts
{
    public sealed record Query(Guid Id, int TopK = 20) : ICommand<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IVectorSearchService vectorSearchService)
        : ICommandHandler<Query, Response>
    {
        /// <summary>
        /// Finds visually similar products using pgvector cosine distance on image embeddings.
        /// </summary>
        /// <param name="request">The query containing the product ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the list of similar product variants.</returns>
        // Contract: pre=request.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Find the variant and its product.
            var variant = await dbContext.Set<Variant>()
                .Include(x => x.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == request.Id && !x.IsDeleted, cancellationToken);

            if (variant is null || variant.Product is null)
                return Result<Response>.NotFound();

            const string similarityModel = VariantImageConstant.Defaults.DefaultSimilarityModel;

            // Load: Get the embedding vector and its model name for the variant's primary image.
            var embeddingData = await dbContext.Set<ImageEmbedding>()
                .Include(ie => ie.VariantImage)
                .Where(ie => ie.VariantImage.VariantId == variant.Id
                          && ie.ModelName == similarityModel)
                .Select(ie => new { ie.Vector, ie.ModelName })
                .FirstOrDefaultAsync(cancellationToken);

            if (embeddingData is null)
                return Result<Response>.Ok(new Response { Items = [] });

            // Query: Find nearest neighbors in vector space using cosine distance.
            var similarVariantIds = await vectorSearchService.FindSimilarVariantIdsAsync(
                embeddingData.Vector, embeddingData.ModelName, request.TopK,
                excludeProductId: variant.ProductId, cancellationToken);

            if (similarVariantIds.Count == 0)
                return Result<Response>.Ok(new Response { Items = [] });

            // Load: Fetch full variant data with includes for response mapping.
            var similarVariants = await dbContext.Set<Variant>()
                .Where(v => similarVariantIds.Contains(v.Id))
                .Include(x => x.Product)
                .Include(x => x.Prices)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Order: Preserve the similarity ranking from the vector search, then apply secondary sort.
            var orderedVariants = similarVariantIds
                .Select(id => similarVariants.First(v => v.Id == id))
                .OrderBy(v => v.Position).ThenBy(v => v.IsMaster ? 0 : 1)
                .ToList();

            // Map: Build response with similar products.
            var items = orderedVariants.Select(v => new SimilarProductItem
            {
                VariantId = v.Id,
                ProductId = v.ProductId,
                ProductName = v.Product?.Name ?? "",
                Sku = v.Sku ?? "",
                Price = v.Price ?? 0
            }).ToList();

            return Result<Response>.Ok(new Response { Items = items });
        }
    }
}