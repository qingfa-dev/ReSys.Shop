using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.Paged;

public static partial class GetTaxonsAllOrPaged
{
    public record Parameters : QueryingParameters
    {
        public Guid TaxonomyId { get; init; }
    }

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

            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == parameters.TaxonomyId, cancellationToken);
            if (!taxonomyExists)
                return PagedResult<Response>.NotFound();

            var query = dbContext.Set<Taxon>()
                .Include(t => t.TaxonRules)
                .Include(t => t.Classifications)
                .Include(t => t.Children)
                .Where(t => t.TaxonomyId == parameters.TaxonomyId)
                .AsNoTracking();

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderBy(t => t.Lft)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}