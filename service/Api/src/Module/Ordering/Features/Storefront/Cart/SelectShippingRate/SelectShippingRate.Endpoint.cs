using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    /// <summary>Maps the storefront shipping-rate selection route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/storefront/cart/shipping-rate — select a shipping rate for the order
            app.MapPatch(OrderingFeature.Storefront.Cart.SelectShippingRate.Route, async (
                [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(SelectShippingRate))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.SelectShippingRate.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.SelectShippingRate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}