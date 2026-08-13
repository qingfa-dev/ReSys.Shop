// GetCartReservations.Endpoint.cs
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Get;

public static partial class GetCartReservations
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.StockReservations.Get.Route, async (
                HttpContext httpContext,
                IStockReservationService reservationService,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                if (string.IsNullOrWhiteSpace(cartToken))
                    return Results.BadRequest(Result.Failure(
                        Error.BadRequest("CartToken.Required", "X-Cart-Token header is required")));

                var result = await reservationService.GetReservationsForCartAsync(cartToken, ct);
                if (result.IsFailure)
                    return result.ToResult();

                var response = result.Value.Select(r => new CartReservationStatus
                {
                    Id = r.Reservation.Id,
                    VariantId = r.Reservation.VariantId,
                    StockLocationId = r.Reservation.StockLocationId,
                    OrderId = r.Reservation.OrderId,
                    Quantity = r.Reservation.Quantity,
                    State = r.Reservation.State.ToString(),
                    ExpiresAtUtc = r.Reservation.ExpiresAtUtc,
                    Reason = r.Reservation.Reason,
                    CreatedAtUtc = r.Reservation.CreatedAtUtc,
                    ModifiedAtUtc = r.Reservation.ModifiedAtUtc,
                    RemainingSeconds = r.RemainingSeconds
                }).ToList();

                return Result<List<CartReservationStatus>>.Ok(response).ToResult();
            })
            .WithName(nameof(GetCartReservations))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Get.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Get.Description)
            .Produces<Result<List<CartReservationStatus>>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
