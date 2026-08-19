using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxons.Get.Tree;

public static partial class GetTaxonTree
{
    public record Parameters : QueryingParameters
    {
        public Guid TaxonomyId { get; init; }
    }
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handler for getting the taxon tree for a taxonomy.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Gets the taxon tree for a taxonomy.</summary>
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<Taxonomy>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Parameters.TaxonomyId, cancellationToken);

            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            var parsedParameters = query.Parameters.ParseAll(
                allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
                allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
                allowedSortFields: TaxonConstant.Query.AllowedSortFields);
            if (parsedParameters.IsFailure)
                return parsedParameters.Errors;

            var tree = await dbContext.Set<Taxon>()
                .Include(t => t.Taxonomy)
                .Include(t => t.TaxonRules)
                .Include(t => t.Classifications)
                .Include(t => t.Children)
                .AsNoTracking()
                .Where(t => t.TaxonomyId == query.Parameters.TaxonomyId)
                .ApplyQuerying(parsedParameters.Value)
                .ToPagedOrAllAsync(parsedParameters.Value, x => x.MapToTreeItem<Response>(), cancellationToken);

            return tree;
        }
    }
}