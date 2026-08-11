using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Storefront.Shipping.Calculate.Route, async (
                [FromQuery] Guid shippingMethodId,
                [FromQuery] Guid orderId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(new Request
                {
                    ShippingMethodId = shippingMethodId,
                    OrderId = orderId
                }), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(CalculateShipping))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .WithSummary(ShippingFeature.Storefront.Shipping.Calculate.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Calculate.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}