using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Storefront.Cart;
public static partial class RemoveCoupon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Storefront.Cart.RemoveCoupon.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .WithName(nameof(RemoveCoupon)).WithTags(PromotionsFeature.Tags.CouponCode)
            .WithSummary(PromotionsFeature.Storefront.Cart.RemoveCoupon.Summary)
            .WithDescription(PromotionsFeature.Storefront.Cart.RemoveCoupon.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
