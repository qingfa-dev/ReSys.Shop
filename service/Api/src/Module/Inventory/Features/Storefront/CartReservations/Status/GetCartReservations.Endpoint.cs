using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

public static partial class GetCartReservations
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.CartReservations.Status.Route, async (
                HttpContext httpContext,
                ISender sender,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Request.Headers["X-Cart-Token"].FirstOrDefault()
                    ?? httpContext.User.FindFirst("cart_token")?.Value
                    ?? string.Empty;

                var query = new Query(cartToken);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCartReservations))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.CartReservations.Status.Summary)
            .WithDescription(InventoryFeature.Storefront.CartReservations.Status.Description)
            .Produces<Result<List<Response>>>();
        }
    }
}
