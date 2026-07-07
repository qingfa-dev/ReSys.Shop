using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingMethods.Deactivate;

public static partial class DeactivateShippingMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(ShippingFeature.Admin.ShippingMethods.Deactivate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeactivateShippingMethod))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .HasPermission(ShippingFeature.Admin.ShippingMethods.Deactivate.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingMethods.Deactivate.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingMethods.Deactivate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
