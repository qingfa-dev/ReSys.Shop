using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockItems.GetAll.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetAllStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.GetAll.Description)
            .Produces<Result<List<Response>>>();
        }
    }
}
