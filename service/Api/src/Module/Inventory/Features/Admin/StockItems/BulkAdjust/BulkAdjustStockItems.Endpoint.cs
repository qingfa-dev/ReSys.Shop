using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.BulkAdjust;

public static partial class BulkAdjustStockItems
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockItems.BulkAdjust.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(BulkAdjustStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.BulkAdjust.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.BulkAdjust.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.BulkAdjust.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
