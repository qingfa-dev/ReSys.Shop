using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.ById;

public static partial class GetShippingMethodById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.ShippingMethods.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetShippingMethodById))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .HasPermission(ShippingFeature.Admin.ShippingMethods.GetById.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingMethods.GetById.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingMethods.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
