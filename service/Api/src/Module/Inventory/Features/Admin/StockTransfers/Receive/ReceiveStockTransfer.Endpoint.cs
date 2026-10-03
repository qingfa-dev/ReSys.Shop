using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.Receive;

public static partial class ReceiveStockTransfer
{
    /// <summary>Receives a stock transfer.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /admin/inventory/stock-transfers/{id}/receive — receives a stock transfer
            app.MapPost(InventoryFeature.Admin.StockTransfers.Receive.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReceiveStockTransfer))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.Receive.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.Receive.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.Receive.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}