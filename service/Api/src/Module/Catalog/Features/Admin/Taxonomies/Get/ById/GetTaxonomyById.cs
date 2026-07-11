using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.ById;

/// <summary>
/// Defines the use case for retrieving a single taxonomy by its ID.
/// </summary>
public static partial class GetTaxonomyById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a single taxonomy by its ID with full details.
        /// </summary>
        /// <param name="request">The query containing the taxonomy ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the taxonomy detail response.</returns>
        // Contract: pre=request.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the taxonomy entity by its ID.
            var entity = await dbContext.Set<Taxonomy>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: If the taxonomy is not found, return a specific error.
            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Map: Return the entity as a detail response.
            return entity.MapToDetail<Response>();
        }
    }

}
