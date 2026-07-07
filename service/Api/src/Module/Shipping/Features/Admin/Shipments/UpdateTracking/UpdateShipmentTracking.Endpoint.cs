using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.UpdateTracking;

public static partial class UpdateShipmentTracking
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ShippingFeature.Admin.Shipments.UpdateTracking.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateShipmentTracking))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.UpdateTracking.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.UpdateTracking.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.UpdateTracking.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
