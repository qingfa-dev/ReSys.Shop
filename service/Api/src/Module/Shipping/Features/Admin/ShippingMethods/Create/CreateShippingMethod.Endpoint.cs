using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;

public static partial class CreateShippingMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ShippingFeature.Admin.ShippingMethods.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateShippingMethod))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .HasPermission(ShippingFeature.Admin.ShippingMethods.Create.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingMethods.Create.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingMethods.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}