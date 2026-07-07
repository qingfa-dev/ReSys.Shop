using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.MethodRates.Get.Paged;

public static partial class GetMethodRates
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.MethodRates.GetAll.Route, async (
                [FromRoute] Guid methodId,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(methodId, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetMethodRates))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.MethodRates.GetAll.Permission)
            .WithSummary(ShippingFeature.Admin.MethodRates.GetAll.Summary)
            .WithDescription(ShippingFeature.Admin.MethodRates.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
