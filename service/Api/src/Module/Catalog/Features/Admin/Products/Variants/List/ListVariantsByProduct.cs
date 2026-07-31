using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

/// <summary>
/// Defines the use case for listing variants by product.
/// </summary>
public static partial class ListVariantsByProduct
{
    public sealed record Query(Guid ProductId, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Lists non-deleted variants for a product, including prices, option-value
    /// associations, and images, paged or all in one page.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=query.ProductId!=Guid.Empty, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var pageModel = PageModelExtensions.FromValues(query.Parameters.PageNumber, query.Parameters.PageSize).Value;

            // Load: Fetch non-deleted variants for product with relations, ordered by position
            return await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .Where(x => x.ProductId == query.ProductId && !x.IsDeleted)
                .OrderBy(x => x.Position)
                .ToPagedOrAllAsync(x => x.MapToDetail<Response>(), pageModel, cancellationToken);
        }
    }
}
