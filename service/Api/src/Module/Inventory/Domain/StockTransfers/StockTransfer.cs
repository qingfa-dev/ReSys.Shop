using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Domain.StockTransfers;

/// <summary>
/// Represents a transfer of inventory between two stock locations.
/// </summary>
// Invariant: State transitions follow Draft → InTransit → Received | Canceled
// Invariant: SourceLocationId != DestinationLocationId
// Contract: pre=sourceLocationId!=Guid.Empty && destinationLocationId!=Guid.Empty, post=Id!=Guid.Empty
public sealed partial class StockTransfer : Entity, IAuditable
{
    #region Properties
    /// <summary>Gets or sets the auto-generated transfer number (e.g., T20260522-0001).</summary>
    public string Number { get; set; } = string.Empty;
    /// <summary>Gets or sets an optional external reference (e.g., PO number, shipment ref).</summary>
    public string? Reference { get; set; }
    /// <summary>Gets or sets the current state of the transfer.</summary>
    public TransferState State { get; set; } = TransferState.Draft;
    /// <summary>Gets or sets the source stock location identifier.</summary>
    public Guid SourceLocationId { get; set; }
    /// <summary>Gets or sets the destination stock location identifier.</summary>
    public Guid DestinationLocationId { get; set; }
    #endregion Properties

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Navigation
    /// <summary>Gets or sets the source stock location.</summary>
    public StockLocation? SourceLocation { get; set; }
    /// <summary>Gets or sets the destination stock location.</summary>
    public StockLocation? DestinationLocation { get; set; }
    /// <summary>Gets or sets the items being transferred.</summary>
    public ICollection<TransferItem> TransferItems { get; set; } = [];
    #endregion Navigation

    #region Constructor
    internal StockTransfer() { }
    #endregion Constructor
}

/// <summary>Represents a single variant line item within a stock transfer.</summary>
public sealed class TransferItem : Entity
{
    /// <summary>Gets or sets the parent stock transfer identifier.</summary>
    public Guid StockTransferId { get; set; }
    /// <summary>Gets or sets the product variant identifier being transferred.</summary>
    public Guid VariantId { get; set; }
    /// <summary>Gets or sets the quantity to transfer.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the quantity received at the destination.</summary>
    public int ReceivedQuantity { get; set; }

    internal TransferItem() { }
}

/// <summary>Lifecycle states for a stock transfer.</summary>
public enum TransferState
{
    Draft,
    InTransit,
    Received,
    Canceled
}