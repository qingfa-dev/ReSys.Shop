using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.Delete;

public static partial class DeleteShipment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ShippingFeature.Admin.Shipments.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteShipment))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.Delete.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.Delete.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
