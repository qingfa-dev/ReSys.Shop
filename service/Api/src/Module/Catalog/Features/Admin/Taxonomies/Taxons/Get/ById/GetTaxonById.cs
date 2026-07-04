using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.ById;

public static partial class GetTaxonById
{
    public sealed record Query(Guid TaxonomyId, Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var taxonomyExists = await dbContext.Set<Taxonomy>()
                .AnyAsync(x => x.Id == request.TaxonomyId, cancellationToken);
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            var entity = await dbContext.Set<Taxon>()
                .Include(x => x.Parent)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.TaxonomyId == request.TaxonomyId, cancellationToken);

            if (entity is null)
                return TaxonResult.Errors.NotFound;

            return entity.MapToDetail<Response>();
        }
    }
}
