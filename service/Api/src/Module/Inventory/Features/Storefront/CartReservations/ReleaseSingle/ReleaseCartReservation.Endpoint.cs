using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public static partial class ReleaseCartReservation
{
    /// <summary>Releases a cart stock reservation scoped to the caller's cart token.</summary>
    /// <remarks>Legacy REST edge — superseded by <c>ReleaseCartStockReservationsCommand</c>. Deprecated; kept for <c>app/Store</c> + <c>ApiTests</c> compatibility.</remarks>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Storefront.Cart.Release.Route, async (
                [FromRoute] Guid reservationId,
                HttpContext httpContext,
                ISender sender,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Request.Headers["X-Cart-Token"].FirstOrDefault()
                    ?? httpContext.User.FindFirst("cart_token")?.Value
                    ?? string.Empty;

                var command = new Command(new Request
                {
                    ReservationId = reservationId,
                    CartToken = cartToken
                });
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReleaseCartReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.Cart.Release.Summary)
            .WithDescription(InventoryFeature.Storefront.Cart.Release.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
