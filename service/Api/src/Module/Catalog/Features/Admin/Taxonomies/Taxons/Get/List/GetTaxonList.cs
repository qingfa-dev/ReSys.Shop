using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxons.Get.List;

public static partial class GetTaxonList
{
    public record Parameters : QueryingParameters;

    public sealed record Query(Guid TaxonomyId, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;
            var parsing = parameters.ParseAll(
                allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
                allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
                allowedSortFields: TaxonConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<Taxon>()
                .Include(t => t.Taxonomy)
                .Include(t => t.TaxonRules)
                .Include(t => t.Classifications)
                .Include(t => t.Children)
                .AsNoTracking()
                .Where(t => t.TaxonomyId == request.TaxonomyId)
                .OrderBy(t => t.Lft)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
