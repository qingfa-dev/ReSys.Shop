using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Storefront.CartReservations.Reserve.Route, async (
                [FromBody] Request request,
                HttpContext httpContext,
                ISender sender,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Request.Headers["X-Cart-Token"].FirstOrDefault()
                    ?? httpContext.User.FindFirst("cart_token")?.Value
                    ?? Guid.NewGuid().ToString("N");

                var command = new Command(request, cartToken);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReserveCartStock))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.CartReservations.Reserve.Summary)
            .WithDescription(InventoryFeature.Storefront.CartReservations.Reserve.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}