using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingRates.Delete;

public static partial class DeleteShippingRate
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ShippingFeature.Admin.ShippingRates.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteShippingRate))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.ShippingRates.Delete.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingRates.Delete.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingRates.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}