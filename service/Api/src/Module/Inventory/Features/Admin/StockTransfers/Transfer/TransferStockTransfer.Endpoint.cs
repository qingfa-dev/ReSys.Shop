using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.Transfer;

public static partial class TransferStockTransfer
{
    /// <summary>Transfers stock between locations.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /admin/inventory/stock-transfers/{id}/transfer — transfers stock between locations
            app.MapPost(InventoryFeature.Admin.StockTransfers.Transfer.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(TransferStockTransfer))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.Transfer.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.Transfer.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.Transfer.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}