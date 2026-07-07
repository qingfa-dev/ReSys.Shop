using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;

public static partial class GetPagedShippingMethods
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.ShippingMethods.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedShippingMethods))
            .WithTags(ShippingFeature.Tags.ShippingMethod)
            .HasPermission(ShippingFeature.Admin.ShippingMethods.GetAll.Permission)
            .WithSummary(ShippingFeature.Admin.ShippingMethods.GetAll.Summary)
            .WithDescription(ShippingFeature.Admin.ShippingMethods.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
