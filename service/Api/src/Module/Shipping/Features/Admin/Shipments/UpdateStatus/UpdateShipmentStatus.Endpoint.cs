using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.UpdateStatus;

public static partial class UpdateShipmentStatus
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ShippingFeature.Admin.Shipments.UpdateStatus.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateShipmentStatus))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.UpdateStatus.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.UpdateStatus.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.UpdateStatus.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
