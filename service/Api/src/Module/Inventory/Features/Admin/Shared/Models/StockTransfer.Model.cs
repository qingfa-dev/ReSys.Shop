namespace Module.Inventory.Features.Admin.Shared.Models;

public abstract record StockTransferParameters
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

public class StockTransferReceiveRequest
{
    public List<ReceiveItemRequest> Items { get; set; } = [];
}

public class ReceiveItemRequest
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}

public record StockTransferRequest : StockTransferParameters
{
    public List<TransferItemRequest> Items { get; set; } = [];
}

public class TransferItemRequest
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}

public record StockTransferDetailResponse : StockTransferParameters
{
    public Guid Id { get; init; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public List<TransferItemResponse> Items { get; set; } = [];
}

public record StockTransferListItemResponse : StockTransferParameters
{
    public Guid Id { get; init; }
    public string Number { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public record TransferItemResponse
{
    public Guid Id { get; init; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
}
