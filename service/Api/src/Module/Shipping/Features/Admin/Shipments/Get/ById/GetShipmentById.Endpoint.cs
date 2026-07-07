using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.Get.ById;

public static partial class GetShipmentById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.Shipments.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetShipmentById))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.GetById.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.GetById.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
