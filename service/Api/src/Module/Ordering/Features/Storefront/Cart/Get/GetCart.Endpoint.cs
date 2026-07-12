using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.Get;

public static partial class GetCart
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Storefront.Cart.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                // Call: Dispatch get-cart query.
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(GetCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Get.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
