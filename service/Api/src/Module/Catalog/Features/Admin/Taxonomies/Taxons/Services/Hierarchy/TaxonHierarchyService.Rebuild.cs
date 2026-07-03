using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

public partial class TaxonHierarchyService
{
    public async Task<Result> RebuildHierarchyAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default)
    {
        // Check: Ensure the target taxonomy exists
        var taxonomyResult = await GetTaxonomyOrFailureAsync(taxonomyId, ct);
        if (taxonomyResult.IsFailure) return taxonomyResult.Errors;

        var taxonomy = taxonomyResult.Value;

        // Initialize: Load the required taxon tree segment (tracking enabled for persistence)
        // We load the full tree if we need to shift, or a subtree if we are only regenerating permalinks.
        // For a full "RebuildHierarchy", it's safer and more consistent to load the full tree.
        var treeResult = await LoadTaxonTreeAsync(taxonomyId, null, asNoTracking: false, ct: ct);
        if (treeResult.IsFailure) return treeResult.Errors;

        var allTaxons = treeResult.Value;
        if (allTaxons.Count == 0) return Result.Ok();

        // Compute: Rebuild nested set values
        var rebuildResult = RebuildNestedSetsInternal(allTaxons, taxonId);
        if (rebuildResult.IsFailure) return rebuildResult;

        // Update: Regenerate permalinks in memory using the updated coordinates
        // We order by Lft to ensure top-down processing
        var sortedTaxons = allTaxons.OrderBy(t => t.Lft).ToList();
        UpdatePermalinksInternal(taxonomy.Name, allTaxons, sortedTaxons);

        // Update: Save all recomputed coordinates and permalinks to the database
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> RebuildNestedSetsAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default)
    {
        // Check: Ensure the target taxonomy exists before processing
        var taxonomyResult = await GetTaxonomyOrFailureAsync(taxonomyId, ct);
        if (taxonomyResult.IsFailure) return taxonomyResult.Errors;

        // Initialize: Always load the FULL taxon tree for the taxonomy to allow shifting coordinates
        var treeResult = await LoadTaxonTreeAsync(taxonomyId, null, asNoTracking: false, ct: ct);
        if (treeResult.IsFailure) return treeResult.Errors;

        var allTaxons = treeResult.Value;
        if (allTaxons.Count == 0) return Result.Ok();

        // Compute: Perform the coordinate reconstruction
        var result = RebuildNestedSetsInternal(allTaxons, taxonId);
        if (result.IsFailure) return result;

        // Update: Save recomputed coordinates back to the persistent store
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>
    /// Core logic for rebuilding nested set coordinates in memory.
    /// Handles full tree rebuilds or shifted subtree updates.
    /// </summary>
    private static Result RebuildNestedSetsInternal(List<Taxon> allTaxons, Guid? taxonId)
    {
        // Map: Create ParentId-to-children lookup for O(1) traversal
        var childrenLookup = allTaxons.ToLookup(t => t.ParentId);

        if (taxonId.HasValue)
        {
            // Filter: Locate the anchor node within the loaded set
            var root = allTaxons.FirstOrDefault(x => x.Id == taxonId.Value);
            if (root == null) return TaxonResult.Errors.NotFound;

            // Assign: Remember the original right boundary before we change it
            int originalRight = root.Rgt;

            // Initialize: Track which nodes were part of the rebuilt branch
            var rebuiltIds = new HashSet<Guid>();

            // Compute: Recursive update starting from the specified anchor
            int newRight = RebuildSubtree(root, childrenLookup, root.Lft, root.Depth, rebuiltIds) - 1;

            // Compute: Calculate the shift required for the rest of the tree
            int delta = newRight - originalRight;

            // Shift: If the size of the subtree changed, shift everything to the right of it
            if (delta != 0)
            {
                // Filter: Find all nodes in the same taxonomy that were NOT part of the rebuilt branch
                var nodesToShift = allTaxons
                    .Where(t => !rebuiltIds.Contains(t.Id))
                    .ToList();

                foreach (var node in nodesToShift)
                {
                    // Update: Adjust coordinates for nodes sitting to the right of the modified branch
                    if (node.Lft > originalRight) node.Lft += delta;
                    if (node.Rgt > originalRight) node.Rgt += delta;
                }
            }
        }
        else
        {
            // Filter: Identify root taxons to begin full tree reconstruction
            var roots = allTaxons.Where(x => x.ParentId == null)
                .OrderBy(x => x.Position)
                .ThenBy(x => x.Name)
                .ToList();

            int currentLeft = 1;
            foreach (var root in roots)
            {
                // Compute: Recursively establish coordinates for each branch
                currentLeft = RebuildSubtree(root, childrenLookup, currentLeft, 0, null);
            }
        }

        return Result.Ok();
    }

    /// <summary>
    /// Recursively rebuilds the nested set values for a subtree.
    /// </summary>
    private static int RebuildSubtree(Taxon taxon, ILookup<Guid?, Taxon> childrenLookup, int left, int depth, HashSet<Guid>? rebuiltIds)
    {
        // Track: Mark node as processed in the current rebuild context
        rebuiltIds?.Add(taxon.Id);

        // Assign: Primary nested set coordinates and hierarchy depth
        taxon.Lft = left;
        taxon.Depth = depth;
        var right = left + 1;

        // Sort: Process children by defined position then name for deterministic ordering
        var children = childrenLookup[taxon.Id]
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Name)
            .ToList();

        foreach (var child in children)
        {
            // Compute: Recursive call to establish child boundaries
            right = RebuildSubtree(child, childrenLookup, right, depth + 1, rebuiltIds);
        }

        // Assign: Final right boundary coordinate
        taxon.Rgt = right;
        return right + 1;
    }
}
