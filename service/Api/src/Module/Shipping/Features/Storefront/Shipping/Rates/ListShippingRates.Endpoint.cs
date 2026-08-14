using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Storefront.Shipping.Rates.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ListShippingRates))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .WithSummary(ShippingFeature.Storefront.Shipping.Rates.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Rates.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}