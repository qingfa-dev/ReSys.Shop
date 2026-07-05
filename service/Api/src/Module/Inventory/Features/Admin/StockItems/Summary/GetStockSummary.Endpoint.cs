using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockItems.StockSummary.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockSummary))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.StockSummary.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.StockSummary.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.StockSummary.Description)
            .Produces<Result<List<Response>>>();
        }
    }
}
