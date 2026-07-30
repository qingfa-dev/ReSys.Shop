using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.Tree;

public static partial class GetTaxonTree
{
    public sealed record Query() : IQuery<Response>;

    /// <summary>Handler for getting the taxon tree.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets the taxon tree for a taxonomy.</summary>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch all taxons with their taxonomy for tree construction
            var entity = await dbContext.Set<Taxonomy>()
                .Include(x => x.Taxons.Where(t => !t.IsDeleted)
                    .OrderBy(t => t.Lft))
                .FirstOrDefaultAsync(cancellationToken);

            if (entity is null)
                return TaxonomyResult.Errors.NotFound;

            // Transform: Flatten root-level taxons into hierarchical tree items
            var tree = entity.Taxons
                .Where(t => t.ParentId is null)
                .Select(t => t.MapToTreeItem<TaxonTreeItem>())
                .ToList();

            return Result<Response>.Ok(new Response { Tree = tree });
        }
    }
}