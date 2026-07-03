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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="request">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Query: Fetch the taxonomy entity by its ID.
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
