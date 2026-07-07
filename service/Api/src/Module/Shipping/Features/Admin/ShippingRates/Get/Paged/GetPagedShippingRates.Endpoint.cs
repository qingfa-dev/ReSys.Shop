using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.Paged;

public static partial class GetPagedShippingRates
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.ShippingRates.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedShippingRates))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.ShippingRates.GetAll.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingRates.GetAll.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingRates.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
