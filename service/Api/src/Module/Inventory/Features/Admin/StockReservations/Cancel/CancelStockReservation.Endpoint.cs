using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockReservations.Cancel;

public static partial class CancelStockReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockReservations.Cancel.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CancelStockReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .HasPermission(InventoryFeature.Admin.StockReservations.Cancel.Permission)
            .WithSummary(InventoryFeature.Admin.StockReservations.Cancel.Summary)
            .WithDescription(InventoryFeature.Admin.StockReservations.Cancel.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
