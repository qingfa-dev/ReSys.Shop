using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.Paged;

/// <summary>
/// Defines the use case for retrieving a paged or full list of taxonomies.
/// </summary>
public static partial class GetTaxonomiesPaged
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Query: Start with the base set of taxonomies.
            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<Taxonomy>()
                .AsNoTracking()
                // Filter: Apply dynamic filtering, sorting, and searching.
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
