using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.CouponCodes.Create;
public static partial class CreateCouponCode
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.CouponCodes.Create.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateCouponCode))
            .WithTags(PromotionsFeature.Tags.CouponCode)
            .HasPermission(PromotionsFeature.Admin.CouponCodes.Create.Permission)
            .WithSummary(PromotionsFeature.Admin.CouponCodes.Create.Summary)
            .WithDescription(PromotionsFeature.Admin.CouponCodes.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
