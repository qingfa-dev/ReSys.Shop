using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.CouponCodes.Get.ById;
public static partial class GetCouponCodeById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.CouponCodes.GetById.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCouponCodeById))
            .WithTags(PromotionsFeature.Tags.CouponCode)
            .HasPermission(PromotionsFeature.Admin.CouponCodes.GetById.Permission)
            .WithSummary(PromotionsFeature.Admin.CouponCodes.GetById.Summary)
            .WithDescription(PromotionsFeature.Admin.CouponCodes.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
