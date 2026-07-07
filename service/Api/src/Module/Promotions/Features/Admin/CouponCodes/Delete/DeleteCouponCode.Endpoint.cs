using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.CouponCodes.Delete;
public static partial class DeleteCouponCode
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Admin.CouponCodes.Delete.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteCouponCode))
            .WithTags(PromotionsFeature.Tags.CouponCode)
            .HasPermission(PromotionsFeature.Admin.CouponCodes.Delete.Permission)
            .WithSummary(PromotionsFeature.Admin.CouponCodes.Delete.Summary)
            .WithDescription(PromotionsFeature.Admin.CouponCodes.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
