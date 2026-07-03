using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

public partial class TaxonHierarchyService
{
    /// <summary>
    /// Ensures the taxonomy exists and returns it.
    /// </summary>
    private async Task<Result<Taxonomy>> GetTaxonomyOrFailureAsync(Guid taxonomyId, CancellationToken ct)
    {
        // Check: Taxonomy exists in the system (including soft-deleted)
        var taxonomy = await _dbContext.Set<Taxonomy>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taxonomyId, ct);

        return taxonomy != null
            ? Result<Taxonomy>.Ok(taxonomy)
            : TaxonomyResult.Errors.NotFound;
    }

    /// <summary>
    /// Gets a taxon by ID and taxonomy ID, ensuring it belongs to the correct taxonomy.
    /// </summary>
    private async Task<Result<Taxon>> GetTaxonOrFailureAsync(Guid taxonId, Guid taxonomyId, CancellationToken ct)
    {
        // Check: Taxon exists and is associated with the specified taxonomy (including soft-deleted)
        var taxon = await _dbContext.Set<Taxon>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taxonId && x.TaxonomyId == taxonomyId, ct);

        return taxon != null
            ? Result<Taxon>.Ok(taxon)
            : TaxonResult.Errors.NotFound;
    }

    /// <summary>
    /// Loads the entire taxon tree or a subtree for a given taxonomy into an optimized in-memory list.
    /// </summary>
    private async Task<Result<List<Taxon>>> LoadTaxonTreeAsync(
        Guid taxonomyId,
        Guid? anchorTaxonId = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        // Initialize: Query for all taxons in the taxonomy (including soft-deleted)
        var query = _dbContext.Set<Taxon>()
            .IgnoreQueryFilters()
            .Where(x => x.TaxonomyId == taxonomyId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (anchorTaxonId.HasValue)
        {
            // Check: Anchor taxon exists to define subtree boundaries
            var anchor = await _dbContext.Set<Taxon>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == anchorTaxonId.Value, ct);

            if (anchor == null)
                return TaxonResult.Errors.NotFound;

            // Filter: Constrain to anchor and its descendants using nested set boundaries
            query = query.Where(x => x.Lft >= anchor.Lft && x.Rgt <= anchor.Rgt);
        }

        // Sort: Order by Lft to facilitate top-down tree traversal in memory
        var taxons = await query
            .OrderBy(x => x.Lft)
            .ToListAsync(ct);

        return Result<List<Taxon>>.Ok(taxons);
    }
}
