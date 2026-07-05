using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Get.Paged;

public static partial class GetPagedStockItems
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockItems.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
