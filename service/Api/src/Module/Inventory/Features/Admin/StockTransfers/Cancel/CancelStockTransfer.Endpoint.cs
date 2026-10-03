using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.Cancel;

public static partial class CancelStockTransfer
{
    /// <summary>Cancels a stock transfer.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /admin/inventory/stock-transfers/{id}/cancel — cancels a stock transfer
            app.MapPost(InventoryFeature.Admin.StockTransfers.Cancel.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CancelStockTransfer))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.Cancel.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.Cancel.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.Cancel.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}