using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Storefront.Cart;
public static partial class ApplyCoupon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Storefront.Cart.ApplyCoupon.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ApplyCoupon)).WithTags(PromotionsFeature.Tags.CouponCode)
            .WithSummary(PromotionsFeature.Storefront.Cart.ApplyCoupon.Summary)
            .WithDescription(PromotionsFeature.Storefront.Cart.ApplyCoupon.Description)
            .Produces<Result>().Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
