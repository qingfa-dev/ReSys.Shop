using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockReservations.Get.Paged;

public static partial class GetPagedStockReservations
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockReservations.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedStockReservations))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .HasPermission(InventoryFeature.Admin.StockReservations.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockReservations.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockReservations.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
