using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Storefront.CartReservations.Release.Route, async (
                [FromRoute] Guid reservationId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(reservationId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReleaseCartReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.CartReservations.Release.Summary)
            .WithDescription(InventoryFeature.Storefront.CartReservations.Release.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}