using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingRates.Update;

public static partial class UpdateShippingRate
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ShippingFeature.Admin.ShippingRates.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateShippingRate))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.ShippingRates.Update.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingRates.Update.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingRates.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
