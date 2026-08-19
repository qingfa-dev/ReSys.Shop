using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Admin.Shared.Models;
using Shared.Operational.Persistence.Specifications.Sorting;

namespace Module.Catalog.Features.Admin.Variants.Images.Get.PagedOrAll;

/// <summary>
/// Defines the use case for listing images by variant.
/// </summary>
public static partial class GetVariantImagePagedOrAll
{
    public sealed record Response : VariantImageDetailResponse;
    public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Handles listing all images for a given variant, ordered by display position.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Executes the query: loads images for the variant ordered by position, paged or all in one page.
        /// </summary>
        // Contract: pre=query!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Validate: Parse and validate query parameters against allowed fields
            var parsing = query.Parameters.ParseAll(
                allowedFilterFields: VariantImageConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: VariantImageConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: VariantImageConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Filter: Load images scoped to the variant with querying, default-sorted by position
            return await dbContext.Set<VariantImage>()
                .Where(x => x.VariantId == query.VariantId)
                .ApplyQuerying(parsing.Value, defaultSortClauses: [new SortClause { Field = nameof(VariantImage.Position) }])
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<Response>(), cancellationToken);
        }
    }
}
