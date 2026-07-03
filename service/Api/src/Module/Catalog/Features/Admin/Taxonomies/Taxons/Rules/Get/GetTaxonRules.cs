using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Get;

/// <summary>
/// Defines the use case for retrieving taxon rules.
/// </summary>
public static partial class GetTaxonRules
{
    public sealed record Query(Guid TaxonomyId, Guid TaxonId) : IQuery<List<Response>>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="query">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=query!=null, post=result!=null
        public async Task<Result<List<Response>>> Handle(Query query, CancellationToken cancellationToken)
        {
            var taxonExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.Id == query.TaxonId && x.TaxonomyId == query.TaxonomyId, cancellationToken);
            if (!taxonExists)
                return TaxonResult.Errors.NotFound;

            var rules = await dbContext.Set<TaxonRule>()
                .Where(x => x.TaxonId == query.TaxonId)
                .OrderBy(x => x.Type)
                .ToListAsync(cancellationToken);

            var mapped = rules.Select(r => r.MapToDetail<Response>()).ToList();
            return mapped;
        }
    }
}
