using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Delete;

public static partial class DeleteStockItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Admin.StockItems.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteStockItem))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Delete.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Delete.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
