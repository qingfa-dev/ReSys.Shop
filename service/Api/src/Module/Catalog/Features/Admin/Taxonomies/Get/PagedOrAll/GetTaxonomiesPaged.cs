using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.PagedOrAll;

/// <summary>
/// Defines the use case for retrieving a paged or full list of taxonomies.
/// </summary>
public static partial class GetTaxonomiesPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged or full list of taxonomies with filtering and sorting support.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of taxonomy list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll(
                allowedFilterFields: TaxonomyConstant.Query.AllowedFilterFields,
                allowedSearchFields: TaxonomyConstant.Query.AllowedSearchFields,
                allowedSortFields: TaxonomyConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            // Filter: Apply dynamic filtering, sorting, and searching
            var pagedResult = await dbContext.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}