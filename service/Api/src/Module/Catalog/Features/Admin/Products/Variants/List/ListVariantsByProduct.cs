using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;
using Shared.Operational.Persistence.Specifications.Sorting;

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
            // Validate: Parse and validate query parameters against allowed fields
            var parsing = query.Parameters.ParseAll(
                allowedFilterFields: VariantConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: VariantConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: VariantConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Fetch non-deleted variants for product with relations and querying, default-sorted by position
            return await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .Where(x => x.ProductId == query.ProductId && !x.IsDeleted)
                .ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(Variant.Position) }])
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<Response>(), cancellationToken);
        }
    }
}
