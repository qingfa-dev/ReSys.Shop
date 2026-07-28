using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.ById;

/// <summary>
/// Defines the use case for retrieving a single taxon by its ID.
/// </summary>
public static partial class GetTaxonById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a single taxon by its ID within a taxonomy with full details.
        /// </summary>
        /// <param name="request">The query containing the taxonomy ID and taxon ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the taxon detail response.</returns>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {

            // Load: Fetch taxon with parent reference for hierarchy context
            var entity = await dbContext.Set<Taxon>()
                .Include(x => x.Taxonomy)
                .Include(x => x.Parent)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: Parent taxonomy must exist
            var taxonomyExists = entity?.Taxonomy != null;
            if (!taxonomyExists)
                return TaxonomyResult.Errors.NotFound;

            if (entity is null)
                return TaxonResult.Errors.NotFound;

            // Map: Return taxon detail response
            return entity.MapToDetail<Response>();
        }
    }
}