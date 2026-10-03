using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.ShippingMethods.Get;

public static partial class GetShippingMethods
{
    /// <summary>Storefront: list available shipping methods.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route} → list shipping methods
            app.MapGet(ShippingFeature.Storefront.Shipping.Methods.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetShippingMethods))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .WithSummary(ShippingFeature.Storefront.Shipping.Methods.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Methods.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}