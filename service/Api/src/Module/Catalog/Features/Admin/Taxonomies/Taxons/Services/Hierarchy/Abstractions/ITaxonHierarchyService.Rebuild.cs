namespace Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;

public partial interface ITaxonHierarchyService
{
    /// <summary>
    /// Rebuilds the entire hierarchy for a taxonomy, including nested sets and permalinks.
    /// </summary>
    /// <param name="taxonomyId">The ID of the taxonomy to rebuild.</param>
    /// <param name="taxonId">Optional: The ID of a specific taxon to start from (rebuilds a subtree).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> RebuildHierarchyAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Recomputes the Lft/Rgt (nested set) values for all taxons in a taxonomy.
    /// </summary>
    /// <param name="taxonomyId">The ID of the taxonomy.</param>
    /// <param name="taxonId">Optional: The ID of a specific taxon to start from (rebuilds a subtree).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> RebuildNestedSetsAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default);
}