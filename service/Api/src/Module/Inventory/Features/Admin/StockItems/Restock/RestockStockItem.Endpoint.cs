using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Restock;

public static partial class RestockStockItem
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockItems.Restock.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RestockStockItem))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Restock.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Restock.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Restock.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}