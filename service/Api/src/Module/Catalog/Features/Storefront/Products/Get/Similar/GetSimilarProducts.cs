using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

/// <summary>
/// Defines the use case for finding visually similar products.
/// </summary>
public static partial class GetSimilarProducts
{
    public sealed record Query(Guid Id) : ICommand<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate: Find the variant and its product.
            var variant = await dbContext.Set<Variant>()
                .Include(x => x.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == request.Id && !x.IsDeleted, cancellationToken);

            if (variant is null || variant.Product is null)
                return Result<Response>.NotFound();

            // Query: Get the embedding vector for the variant's primary image.
            var queryVector = await dbContext.Set<ImageEmbedding>()
                .Include(ie => ie.VariantImage)
                .Where(ie => ie.VariantImage.VariantId == request.Id)
                .Select(ie => ie.Vector)
                .FirstOrDefaultAsync(cancellationToken);

            if (queryVector is null)
                return Result<Response>.Ok(new Response { Items = [] });

            // Query: Find visually similar variants using cosine distance.
            // Using raw SQL for pgvector distance operator.
            var similarVariants = await dbContext.Set<Variant>()
                .FromSqlRaw(@"
                    SELECT DISTINCT v.*
                    FROM ""Variants"" v
                    INNER JOIN ""VariantImages"" vi ON vi.""VariantId"" = v.""Id""
                    INNER JOIN ""ImageEmbeddings"" ie ON ie.""VariantImageId"" = vi.""Id""
                    WHERE v.""ProductId"" != {0}
                      AND v.""IsDeleted"" = false
                      AND vi.""Type"" = 'Default'
                    ORDER BY ie.""Vector"" <=> {1}::vector
                    LIMIT 20",
                    variant.ProductId, queryVector)
                .Include(x => x.Product)
                .Include(x => x.Prices)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Build response with similar products.
            var items = similarVariants.Select(v => new SimilarProductItem
            {
                VariantId = v.Id,
                ProductId = v.ProductId,
                ProductName = v.Product?.Name ?? "",
                Sku = v.Sku ?? "",
                Price = v.Price ?? 0
            }).ToList();

            return new Response { Items = items };
        }
    }
}
