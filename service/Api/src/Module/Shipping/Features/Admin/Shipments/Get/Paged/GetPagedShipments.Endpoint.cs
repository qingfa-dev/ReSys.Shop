using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.Get.Paged;

public static partial class GetPagedShipments
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.Shipments.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedShipments))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.GetAll.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.GetAll.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
