namespace Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;

public partial interface ITaxonHierarchyService
{
    /// <summary>
    /// Checks if a potential parent is a descendant of the taxon being moved, which would cause a circular dependency.
    /// </summary>
    /// <param name="taxonId">The ID of the taxon being moved/updated.</param>
    /// <param name="potentialParentId">The ID of the potential new parent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating if the hierarchy is valid.</returns>
    Task<Result> ValidateDescendantAsync(Guid taxonId, Guid potentialParentId, CancellationToken ct = default);

    /// <summary>
    /// Validates the integrity of the taxon hierarchy, checking for cycles and root correctness.
    /// </summary>
    /// <param name="taxonomyId">The ID of the taxonomy to validate.</param>
    /// <param name="anchorTaxonId">Optional: The ID of a specific taxon to start validation from (validates a subtree).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating if the hierarchy is valid.</returns>
    Task<Result> ValidateHierarchyAsync(Guid taxonomyId, Guid? anchorTaxonId = null, CancellationToken ct = default);
}