using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Storefront.Taxons.Get.All;

/// <summary>
/// Defines the use case for retrieving all taxons.
/// </summary>
public static partial class GetAllTaxons
{
    public record Parameters : QueryingParameters
    {
        public int? Depth { get; init; }
        public Guid? TaxonomyId { get; init; }
    }

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

            // Filter: Active taxons with optional depth and taxonomy constraints
            var query = dbContext.Set<Taxon>()
                .Where(t => !t.IsDeleted)
                .AsNoTracking();

            if (parameters.Depth.HasValue)
                query = query.Where(t => t.Depth == parameters.Depth.Value);

            if (parameters.TaxonomyId.HasValue)
                query = query.Where(t => t.TaxonomyId == parameters.TaxonomyId.Value);

            // Parse: Validate and parse querying parameters for filtering, searching, and sorting
            var parsing = parameters.ParseAll(
                allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
                allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
                allowedSortFields: TaxonConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            // Compute: Apply nested set ordering and pagination to produce flat taxon list
            var pagedResult = await query
                .OrderBy(t => t.Lft)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => new Response
                {
                    Id = x.Id,
                    Name = x.Name,
                    Presentation = x.Presentation,
                    Permalink = x.Permalink,
                    Depth = x.Depth,
                    ParentId = x.ParentId,
                    Position = x.Position,
                    TaxonCount = x.Children.Count,
                }, cancellationToken);

            return pagedResult;
        }
    }
}