using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockItems.LowStock.Route, async (
                [FromQuery] Guid? locationId,
                [FromQuery] int? threshold,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(locationId, threshold);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetLowStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.LowStock.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.LowStock.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.LowStock.Description)
            .Produces<Result<List<Response>>>();
        }
    }
}
