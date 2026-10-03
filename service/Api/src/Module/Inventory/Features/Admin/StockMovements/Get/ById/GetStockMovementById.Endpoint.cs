using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockMovements.Get.ById;

public static partial class GetStockMovementById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockMovements.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockMovementById))
            .WithTags(InventoryFeature.Tags.StockMovement)
            .HasPermission(InventoryFeature.Admin.StockMovements.GetById.Permission)
            .WithSummary(InventoryFeature.Admin.StockMovements.GetById.Summary)
            .WithDescription(InventoryFeature.Admin.StockMovements.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}