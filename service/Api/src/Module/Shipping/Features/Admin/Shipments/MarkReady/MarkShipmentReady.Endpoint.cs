using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.MarkReady;

public static partial class MarkShipmentReady
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ShippingFeature.Admin.Shipments.MarkReady.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(MarkShipmentReady))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.MarkReady.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.MarkReady.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.MarkReady.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
