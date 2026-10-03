using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.ListForOrder;

public static partial class GetShipmentsForOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.Shipments.ListForOrder.Route, async (
                [FromQuery] Guid orderId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(new Parameters { OrderId = orderId }), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetShipmentsForOrder))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.ListForOrder.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.ListForOrder.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.ListForOrder.Description)
            .Produces<Result<Response>>();
        }
    }
}
