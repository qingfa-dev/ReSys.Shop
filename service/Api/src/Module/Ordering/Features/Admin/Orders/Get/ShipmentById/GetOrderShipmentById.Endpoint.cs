using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.ShipmentById;

public static partial class GetOrderShipmentById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetShipmentById.Route, async (Guid orderId, Guid shipmentId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(orderId, shipmentId), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderShipmentById))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetShipmentById.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetShipmentById.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetShipmentById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
