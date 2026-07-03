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
        /// <inheritdoc />
        // Contract: pre=none, post=result.Items!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Taxon>()
                .Where(t => !t.IsDeleted)
                .AsNoTracking();

            if (parameters.Depth.HasValue)
                query = query.Where(t => t.Depth == parameters.Depth.Value);

            if (parameters.TaxonomyId.HasValue)
                query = query.Where(t => t.TaxonomyId == parameters.TaxonomyId.Value);

            // Parse: Validate and parse querying parameters
            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

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
