using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockReservations.Get.ById;

public static partial class GetStockReservationById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockReservations.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockReservationById))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .HasPermission(InventoryFeature.Admin.StockReservations.GetById.Permission)
            .WithSummary(InventoryFeature.Admin.StockReservations.GetById.Summary)
            .WithDescription(InventoryFeature.Admin.StockReservations.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}