using Module.Catalog.Domain.Taxons;
using Module.Catalog.Domain.Taxons.Rules;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Get;

public static partial class GetTaxonRules
{
    public record Parameters : QueryingParameters;

    public sealed record Query(Guid TaxonId, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var taxonExists = await dbContext.Set<Taxon>()
                .AnyAsync(x => x.Id == query.TaxonId, cancellationToken);
            if (!taxonExists)
                return TaxonResult.Errors.NotFound;

            var parameters = query.Parameters;
            var parsing = parameters.ParseAll(
                allowedFilterFields: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Type", "MatchPolicy", "Value" },
                allowedSearchFields: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Value" },
                allowedSortFields: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Type", "MatchPolicy", "Value" });
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<TaxonRule>()
                .AsNoTracking()
                .Where(x => x.TaxonId == query.TaxonId)
                .OrderBy(x => x.Type)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToDetail<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
