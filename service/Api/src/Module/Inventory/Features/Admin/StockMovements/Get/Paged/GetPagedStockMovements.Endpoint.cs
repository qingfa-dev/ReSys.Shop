using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockMovements.Get.Paged;

public static partial class GetPagedStockMovements
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockMovements.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedStockMovements))
            .WithTags(InventoryFeature.Tags.StockMovement)
            .HasPermission(InventoryFeature.Admin.StockMovements.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockMovements.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockMovements.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}