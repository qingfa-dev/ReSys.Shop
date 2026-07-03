using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

public partial class TaxonHierarchyService
{
    public async Task<Result> ValidateDescendantAsync(Guid taxonId, Guid potentialParentId, CancellationToken ct = default)
    {
        // Check: Taxon being moved must exist in the system
        var taxon = await _dbContext.Set<Taxon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taxonId, ct);

        if (taxon == null) return TaxonResult.Errors.NotFound;

        // Check: Potential parent taxon must exist in the system
        var potentialParent = await _dbContext.Set<Taxon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == potentialParentId, ct);

        if (potentialParent == null) return TaxonResult.Errors.NotFound;

        // Enforce: Both taxons must belong to the same taxonomy for a valid move
        if (taxon.TaxonomyId != potentialParent.TaxonomyId)
            return TaxonResult.Errors.ParentTaxonomyMismatch;

        // Validate: Potential parent cannot be a descendant of the taxon being moved (prevents circularity)
        if (potentialParent.Lft >= taxon.Lft && potentialParent.Rgt <= taxon.Rgt)
            return TaxonResult.Errors.CircularParenting;

        return Result.Ok();
    }

    public async Task<Result> ValidateHierarchyAsync(Guid taxonomyId, Guid? anchorTaxonId = null, CancellationToken ct = default)
    {
        // Check: Ensure the taxonomy exists before validating its hierarchy
        var taxonomyResult = await GetTaxonomyOrFailureAsync(taxonomyId, ct);
        if (taxonomyResult.IsFailure) return taxonomyResult.Errors;

        // Initialize: Load the relevant taxon branch or tree for in-memory analysis
        var treeResult = await LoadTaxonTreeAsync(taxonomyId, anchorTaxonId, asNoTracking: true, ct: ct);
        if (treeResult.IsFailure) return treeResult.Errors;

        // Verify: Assert structural integrity including cycle detection and boundary consistency
        return VerifyStructuralIntegrity(treeResult.Value);
    }

    /// <summary>
    /// Verifies the structural integrity of the taxon hierarchy in memory.
    /// Checks for orphans, cycles, and nested set consistency.
    /// </summary>
    private static Result VerifyStructuralIntegrity(List<Taxon> taxons)
    {
        if (taxons.Count == 0) return Result.Ok();

        // Map: Create ID-to-entity lookup and ParentId-to-children lookup for O(1) traversal
        var taxonMap = taxons.ToDictionary(t => t.Id);
        var childrenLookup = taxons.ToLookup(t => t.ParentId);

        // Filter: Identify root nodes (no parent or parent outside the loaded set)
        var roots = taxons.Where(t => t.ParentId == null || !taxonMap.ContainsKey(t.ParentId.Value)).ToList();

        // Check: Ensure at least one root is present in the hierarchy
        if (roots.Count == 0 && taxons.Count > 0)
            return TaxonResult.Errors.NoRoot;

        // Initialize: Sets to track nested set boundary usage for overlap detection
        var usedBoundaries = new HashSet<int>();

        foreach (var taxon in taxons)
        {
            // Validate: Nested set boundary consistency (Lft must be less than Rgt)
            if (taxon.Lft >= taxon.Rgt)
                return TaxonResult.Errors.InvalidNestedSet(taxon.Name, taxon.Id, taxon.Lft, taxon.Rgt);

            // Validate: Ensure no duplicate or overlapping boundaries exist (unique Lft/Rgt)
            if (!usedBoundaries.Add(taxon.Lft) || !usedBoundaries.Add(taxon.Rgt))
                return TaxonResult.Errors.OverlappingBoundaries(taxon.Name, taxon.Id);
        }

        // Initialize: Detect hierarchical cycles using depth-first traversal from roots
        var visited = new HashSet<Guid>();
        var currentPath = new HashSet<Guid>();

        foreach (var root in roots)
        {
            var result = DetectAndVerifySubtree(root.Id, 0, taxonMap, childrenLookup, visited, currentPath);
            if (result.IsFailure) return result;
        }

        return Result.Ok();
    }

    private static Result DetectAndVerifySubtree(
        Guid taxonId, int expectedDepth, 
        Dictionary<Guid, Taxon> map,
        ILookup<Guid?, Taxon> childrenLookup,
        HashSet<Guid> visited, 
        HashSet<Guid> currentPath)
    {
        var taxon = map[taxonId];

        // Enforce: No circular parenting (a node cannot appear twice in its own branch)
        if (currentPath.Contains(taxonId))
            return TaxonResult.Errors.CycleDetected;

        if (visited.Contains(taxonId)) return Result.Ok();

        visited.Add(taxonId);
        currentPath.Add(taxonId);

        // Filter: Find all children mapped to this parent from the optimized lookup
        var children = childrenLookup[taxonId].OrderBy(t => t.Lft).ToList();

        foreach (var child in children)
        {
            // Validate: Child boundaries must be contained within parent boundaries
            if (child.Lft <= taxon.Lft || child.Rgt >= taxon.Rgt)
                return TaxonResult.Errors.BoundaryViolation(child.Name, taxon.Name);

            // Compute: Recursive call to establish child branch integrity
            var result = DetectAndVerifySubtree(child.Id, expectedDepth + 1, map, childrenLookup, visited, currentPath);
            if (result.IsFailure) return result;
        }

        currentPath.Remove(taxonId);
        return Result.Ok();
    }
}
