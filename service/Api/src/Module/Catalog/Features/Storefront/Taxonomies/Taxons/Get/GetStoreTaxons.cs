using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Classifications.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Taxonomies.Taxons.Get;

/// <summary>
/// Defines the use case for retrieving a paged or full list of taxons.
/// </summary>
public static partial class GetStoreTaxons
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a paged or full list of taxons for a taxonomy with filtering and sorting support.
        /// </summary>
        /// <param name="request">The query containing the taxonomy ID, pagination, and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A paged result of taxon list items.</returns>
        // Contract: pre=request.Parameters!=null, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;
            // Parse: Validate and parse querying parameters for filtering, searching, and sorting
            var parsing = parameters.ParseAll(
                allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
                allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
                allowedSortFields: TaxonConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            // Compute: Apply nested set ordering and pagination to produce the paged result
            var pagedResult = await dbContext.Set<Taxon>()
                .Include(t => t.Taxonomy)
                .Include(t => t.TaxonRules)
                .Include(t => t.Classifications)
                .Include(t => t.Children)
                .AsNoTracking()
                .OrderBy(t => t.Lft)
                .Where(t => parameters.Depth == null || t.Depth == parameters.Depth)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}