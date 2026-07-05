using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Update;

public static partial class UpdateStockItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(InventoryFeature.Admin.StockItems.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateStockItem))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Update.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Update.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
