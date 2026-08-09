using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    /// <summary>Reserves stock for a shopping cart.</summary>
    /// <remarks>Legacy REST edge — superseded by <c>ReserveCartStockCommand</c> (cart-scoped, multi-location). Deprecated; kept for <c>app/Store</c> + <c>ApiTests</c> compatibility.</remarks>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefrontapi/storefront/inventory/cart/reservations — reserves stock for a shopping cart
            app.MapPost(InventoryFeature.Storefront.Cart.Reserve.Route, async (
                [FromBody] Request request,
                HttpContext httpContext,
                ISender sender,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Request.Headers["X-Cart-Token"].FirstOrDefault()
                    ?? httpContext.User.FindFirst("cart_token")?.Value
                    ?? Guid.NewGuid().ToString("N");

                request.CartToken = cartToken;
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReserveCartStock))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.Cart.Reserve.Summary)
            .WithDescription(InventoryFeature.Storefront.Cart.Reserve.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}