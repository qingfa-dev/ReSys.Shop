using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingRates.Create;

public static partial class CreateShippingRate
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ShippingFeature.Admin.ShippingRates.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateShippingRate))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.ShippingRates.Create.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingRates.Create.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingRates.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}