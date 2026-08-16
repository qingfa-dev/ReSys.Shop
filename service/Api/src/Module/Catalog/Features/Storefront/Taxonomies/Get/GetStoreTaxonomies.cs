using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Storefront.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Taxonomies.Get;

/// <summary>
/// Defines the use case for retrieving all taxons.
/// </summary>
public static partial class GetStoreTaxonomies
{

    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Retrieves a flat list of taxons filtered by depth and/or taxonomy ID
    /// for breadcrumb resolution and filter panel population.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves all active taxons filtered by depth and/or taxonomy ID for breadcrumb and filter panel population.
        /// </summary>
        /// <param name="request">The query containing optional depth and taxonomy ID filters with pagination parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of flat taxon list items.</returns>
        // Contract: pre=none, post=result.Items!=null
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
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}