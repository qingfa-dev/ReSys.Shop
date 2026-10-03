namespace Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;


/// <summary>
/// Orchestrates automatic product ↔ taxon classification.
///
/// All methods run inside the ambient DbContext; the caller is responsible
/// for any outer transaction when batching multiple operations.
/// Manual classifications (<c>IsAutomatic = false</c>) are never modified.
/// </summary>
public interface IAutoClassificationService
{
    /// <summary>
    /// Re-evaluates every product against a single taxon's rules and refreshes
    /// all automatic classifications for that taxon.
    /// Clears <c>MarkedForRegenerateTaxonProducts</c> on the taxon when done.
    /// </summary>
    Task RegenerateForTaxonAsync(Guid taxonId, CancellationToken ct = default);

    /// <summary>
    /// Re-evaluates a single product against all automatic taxons.
    /// Called when a product's properties change.
    /// </summary>
    Task RegenerateForProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Batch entry point used by the background job:
    /// processes every taxon where <c>MarkedForRegenerateTaxonProducts = true</c>.
    /// </summary>
    Task RegenerateDirtyAsync(CancellationToken ct = default);
}