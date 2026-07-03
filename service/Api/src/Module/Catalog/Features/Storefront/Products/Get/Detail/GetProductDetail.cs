using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

/// <summary>
/// Defines the use case for retrieving product detail by slug.
/// </summary>
public static partial class GetProductDetail
{
    public sealed record Query(string Slug) : IQuery<Response>;

public record Response : StoreProductDetailResponse;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ILogger<QueryHandler> logger) : IQueryHandler<Query, Response>
    {
        /// <inheritdoc />
        // Contract: pre=query.Slug!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .FirstOrDefaultAsync(x => x.Slug == query.Slug
                    && !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow, cancellationToken);

            if (entity is null)
            {
                // Log: Record product not found for observability
                ProductLoggers.StorefrontProductNotFoundBySlug(logger, query.Slug);
                return ProductResult.Errors.NotFoundBySlug(query.Slug);
            }

            // Log: Record product detail loaded for observability
            ProductLoggers.StorefrontProductDetailLoaded(logger, query.Slug, entity.Id);

            return entity.MapToStoreDetail<Response>();
        }
    }
}
