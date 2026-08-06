namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

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

public record TransferItemResponse : Response
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
}