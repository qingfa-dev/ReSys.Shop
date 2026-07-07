using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.ById;

public static partial class GetShippingRateById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.ShippingRates.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetShippingRateById))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.ShippingRates.GetById.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingRates.GetById.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingRates.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
