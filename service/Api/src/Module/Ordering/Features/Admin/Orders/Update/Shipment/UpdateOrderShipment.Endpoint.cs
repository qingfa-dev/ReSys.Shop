using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Update.Shipment;

public static partial class UpdateOrderShipment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateShipment.Route, async (Guid orderId, Guid shipmentId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(orderId, shipmentId, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderShipment))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateShipment.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateShipment.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateShipment.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
