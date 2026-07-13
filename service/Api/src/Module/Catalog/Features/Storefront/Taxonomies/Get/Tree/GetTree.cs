using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Storefront.Taxonomies.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Taxonomies.Get.Tree;

/// <summary>
/// Defines the use case for retrieving the taxonomy tree.
/// </summary>
public static partial class GetTree
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Retrieves the taxonomy tree with nested taxon hierarchy for storefront mega-menu navigation.
    /// Uses Nested Set model (Lft/Rgt) to reconstruct the tree structure.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the taxonomy tree query — loads taxonomy with ordered taxons,
        /// filters hidden/deleted taxons, and maps to a nested tree DTO.
        /// </summary>
        /// <param name="query">The query containing the taxonomy ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the taxonomy tree.</returns>
        // Contract: pre=query.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<Taxonomy>()
                .Include(x => x.Taxons.Where(t => !t.IsDeleted && !t.HideFromNav)
                    .OrderBy(t => t.Lft))
                .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            return Result<Response>.Ok(
                entity.MapToStoreTree<Response>());
        }
    }
}