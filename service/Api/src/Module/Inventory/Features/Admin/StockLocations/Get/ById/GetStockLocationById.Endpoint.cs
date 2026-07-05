using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.Get.ById;

public static partial class GetStockLocationById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockLocations.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockLocationById))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.GetById.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.GetById.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
