using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Storefront.Shipping.Methods.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetShippingMethods))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .WithSummary(ShippingFeature.Storefront.Shipping.Methods.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Methods.Description)
            .Produces<Result<Response>>();
        }
    }
}