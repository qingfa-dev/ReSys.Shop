using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxons.Services.Hierarchy;

public partial class TaxonHierarchyService
{
    /// <summary>Regenerates SEO-friendly permalinks and pretty names for all taxons in a taxonomy (or a subtree).</summary>
    /// <param name="taxonomyId">The taxonomy identifier providing the root name.</param>
    /// <param name="taxonId">Optional anchor taxon to regenerate a subtree only.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A success result or an error if the taxonomy was not found.</returns>
    public async Task<Result> RegeneratePermalinksAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default)
    {
        // Check: Ensure the target taxonomy exists to provide the root name for permalinks
        var taxonomyResult = await GetTaxonomyOrFailureAsync(taxonomyId, ct);
        if (taxonomyResult.IsFailure) return taxonomyResult.Errors;

        var taxonomy = taxonomyResult.Value;

        // Initialize: Always load the FULL taxon tree for the taxonomy
        // This ensures that even if we are only regenerating a subtree, the parent permalinks
        // are available in memory to construct the full path correctly.
        var treeResult = await LoadTaxonTreeAsync(taxonomyId, null, asNoTracking: false, ct: ct);
        if (treeResult.IsFailure) return treeResult.Errors;

        var allTaxons = treeResult.Value;

        // Filter: If an anchor is provided, identify the specific subset of taxons to update
        List<Taxon> taxonsToUpdate;
        if (taxonId.HasValue)
        {
            var anchor = allTaxons.FirstOrDefault(x => x.Id == taxonId.Value);
            if (anchor == null) return TaxonResult.Errors.NotFound;

            taxonsToUpdate = allTaxons
                .Where(x => x.Lft >= anchor.Lft && x.Rgt <= anchor.Rgt)
                .OrderBy(x => x.Lft)
                .ToList();
        }
        else
        {
            taxonsToUpdate = allTaxons.OrderBy(x => x.Lft).ToList();
        }

        // Update: Perform the permalink regeneration in memory
        UpdatePermalinksInternal(taxonomy.Name, allTaxons, taxonsToUpdate);

        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    /// <summary>
    /// Core logic for updating permalinks and pretty names in memory.
    /// Expects taxons to be ordered by Lft to ensure parent data is available for children.
    /// </summary>
    private static void UpdatePermalinksInternal(string taxonomyName, List<Taxon> allTaxons, List<Taxon> taxonsToUpdate)
    {
        // Map: Create a lookup to wire up parent navigation properties in memory using the FULL set
        var map = allTaxons.ToDictionary(t => t.Id);

        foreach (var taxon in taxonsToUpdate)
        {
            // Link: Connect to parent if available in the full set to enable recursive permalink construction
            if (taxon.ParentId.HasValue && map.TryGetValue(taxon.ParentId.Value, out var parent))
            {
                taxon.Parent = parent;
            }

            // Update: Recompute SEO-friendly permalink and human-readable path
            taxon.UpdatePermalink(taxonomyName);
            taxon.UpdatePrettyName(taxonomyName);
        }
    }
}