using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Shipments.Get;
public static partial class GetOrderShipments
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.Shipments.Route, async (
                Guid orderId,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(orderId, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOrderShipments))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Shipments.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Shipments.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Shipments.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
