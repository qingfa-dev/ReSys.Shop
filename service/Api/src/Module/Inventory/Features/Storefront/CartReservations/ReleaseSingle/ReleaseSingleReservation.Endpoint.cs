using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public static partial class ReleaseSingleReservation
{
    /// <summary>Releases a single cart stock reservation.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Storefront.CartReservations.Release.Route, async (
                Guid reservationId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new ReleaseSingleReservationCommand { ReservationId = reservationId };
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReleaseSingleReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.CartReservations.Release.Summary)
            .WithDescription(InventoryFeature.Storefront.CartReservations.Release.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
