using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

public static partial class GetCartReservations
{
    /// <summary>Gets cart stock reservations.</summary>
    /// <remarks>Legacy REST edge — superseded by <c>ReserveCartStockCommand</c>/<c>ConsumeCartStockReservationsCommand</c> orchestration. Deprecated; kept for <c>app/Store</c> + <c>ApiTests</c> compatibility.</remarks>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/inventory/cart/reservations — gets cart stock reservations
            app.MapGet(InventoryFeature.Storefront.Cart.Status.Route, async (
                [AsParameters] Parameters parameters,
                HttpContext httpContext,
                ISender sender,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Request.Headers["X-Cart-Token"].FirstOrDefault()
                    ?? httpContext.User.FindFirst("cart_token")?.Value
                    ?? string.Empty;

                var query = new Query(cartToken, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetCartReservations))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.Cart.Status.Summary)
            .WithDescription(InventoryFeature.Storefront.Cart.Status.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}