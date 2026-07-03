namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

public partial interface ITaxonHierarchyService
{
    /// <summary>
    /// Regenerates the permalinks for taxons based on their hierarchy.
    /// </summary>
    /// <param name="taxonomyId">The ID of the taxonomy.</param>
    /// <param name="taxonId">Optional: The ID of a specific taxon to start from (regenerates for a subtree).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> RegeneratePermalinksAsync(
        Guid taxonomyId,
        Guid? taxonId = null,
        CancellationToken ct = default);
}