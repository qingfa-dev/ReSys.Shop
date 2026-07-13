using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingMethods.Delete;

public static partial class DeleteShippingMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ShippingFeature.Admin.ShippingMethods.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteShippingMethod))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .HasPermission(ShippingFeature.Admin.ShippingMethods.Delete.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingMethods.Delete.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingMethods.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}