namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

/// <summary>Shared parameters for stock transfer requests and responses.</summary>
public abstract class StockTransferParameters
{
    /// <summary>Gets an optional external reference.</summary>
    public string? Reference { get; init; }
    /// <summary>Gets the source stock location identifier.</summary>
    public Guid SourceLocationId { get; init; }
    /// <summary>Gets the destination stock location identifier.</summary>
    public Guid DestinationLocationId { get; init; }
    /// <summary>Gets the current state of the transfer.</summary>
    public string? State { get; init; }
}