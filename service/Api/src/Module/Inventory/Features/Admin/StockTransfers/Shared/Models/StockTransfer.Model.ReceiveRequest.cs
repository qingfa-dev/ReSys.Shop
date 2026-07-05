namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

public class StockTransferReceiveRequest
{
    public List<ReceiveItemRequest> Items { get; set; } = [];
}

public class ReceiveItemRequest
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}
