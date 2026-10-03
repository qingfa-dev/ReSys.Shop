using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    /// <summary>Gets stock summary statistics.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /admin/inventory/stock-items/summary — gets stock summary statistics
            app.MapGet(InventoryFeature.Admin.StockItems.StockSummary.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStockSummary))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.StockSummary.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.StockSummary.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.StockSummary.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}