using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.Inventory.Domain.StockLocations.StockItems;

/// <summary>
/// Represents a stock item tracking inventory for a specific variant at a stock location.
/// </summary>
// Invariant: CountOnHand >= 0; Backorderable is consistent with stock location default
// Contract: pre=stockLocationId!=Guid.Empty && variantId!=Guid.Empty, post=Id!=Guid.Empty
public sealed partial class StockItem : Entity, IAuditable
{
    #region Properties
    /// <summary>Gets or sets the current quantity of stock available for this variant at this location.</summary>
    public int CountOnHand { get; set; } = StockItemConstant.Defaults.CountOnHand;
    /// <summary>Gets or sets whether backorders are allowed for this stock item.</summary>
    public bool Backorderable { get; set; } = StockItemConstant.Defaults.Backorderable;
    /// <summary>Gets or sets the stock location identifier where this inventory is stored.</summary>
    public Guid StockLocationId { get; set; }
    /// <summary>Gets or sets the product variant identifier that this stock item tracks.</summary>
    public Guid VariantId { get; set; }
    /// <summary>Concurrency token for optimistic locking — auto-managed by EF Core / PostgreSQL xid.</summary>
    public uint RowVersion { get; set; }
    #endregion Properties

    #region Auditing
    /// <summary>Gets or sets the UTC timestamp when this stock item was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Gets or sets the UTC timestamp when this stock item was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    /// <summary>Gets or sets the username or identifier of the user who created this stock item.</summary>
    public string? CreatedBy { get; set; }
    /// <summary>Gets or sets the username or identifier of the user who last modified this stock item.</summary>
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Relationships
    /// <summary>Gets or sets the stock location associated with this stock item.</summary>
    public StockLocation? StockLocation { get; set; }
    /// <summary>Gets or sets the collection of stock movements (transactions) for this stock item.</summary>
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    #endregion Relationships

    #region Constructor
    internal StockItem() { } // For EF Core
    #endregion Constructor
}
