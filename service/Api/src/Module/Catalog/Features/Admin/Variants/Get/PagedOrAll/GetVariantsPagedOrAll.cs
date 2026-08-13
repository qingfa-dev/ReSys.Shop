using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Variants.Get.PagedOrAll;

/// <summary>
/// Defines the use case for listing variants by product.
/// </summary>
public static partial class GetVariantsPagedOrAll
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

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
                .Where(x => !x.IsDeleted && (query.Parameters.ProductId == null || x.ProductId == query.Parameters.ProductId))
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);
        }
    }
}
