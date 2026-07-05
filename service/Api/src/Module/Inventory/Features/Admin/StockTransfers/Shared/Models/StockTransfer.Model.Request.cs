namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

public class StockTransferRequest : StockTransferParameters
{
    public List<TransferItemRequest> Items { get; set; } = [];
}

public class TransferItemRequest
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}
