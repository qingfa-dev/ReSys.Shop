using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.CouponCodes.Get.Paged;
public static partial class GetPagedCouponCodes
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.CouponCodes.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedCouponCodes))
            .WithTags(PromotionsFeature.Tags.CouponCode)
            .HasPermission(PromotionsFeature.Admin.CouponCodes.GetAll.Permission)
            .WithSummary(PromotionsFeature.Admin.CouponCodes.GetAll.Summary)
            .WithDescription(PromotionsFeature.Admin.CouponCodes.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
