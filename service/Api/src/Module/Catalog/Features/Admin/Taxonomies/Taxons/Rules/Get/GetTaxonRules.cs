using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Get;

/// <summary>
/// Defines the use case for retrieving taxon rules.
/// </summary>
public static partial class GetTaxonRules
{
    public sealed record Query(Guid TaxonId) : IQuery<List<Response>>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>
        /// Retrieves all rules for a specific taxon ordered by rule type.
        /// </summary>
        /// <param name="query">The query containing the taxonomy ID and taxon ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the list of taxon rule details.</returns>
        // Contract: pre=query!=null, post=result!=null
        public async Task<Result<List<Response>>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Check: Parent taxon must exist before retrieving its rules
            var taxonExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.Id == query.TaxonId, cancellationToken);
            if (!taxonExists)
                return TaxonResult.Errors.NotFound;

            // Load: Fetch all rules for the taxon ordered by rule type
            var rules = await dbContext.Set<TaxonRule>()
                .Where(x => x.TaxonId == query.TaxonId)
                .OrderBy(x => x.Type)
                .ToListAsync(cancellationToken);

            // Map: Transform each rule entity to detail response DTO
            var mapped = rules.Select(r => r.MapToDetail<Response>()).ToList();
            return mapped;
        }
    }
}