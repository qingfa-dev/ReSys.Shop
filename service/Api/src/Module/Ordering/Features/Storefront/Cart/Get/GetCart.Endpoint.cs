using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.Get;

public static partial class GetCart
{
    /// <summary>Maps the storefront cart retrieval route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/cart — retrieve the current shopping cart
            app.MapGet(OrderingFeature.Storefront.Cart.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(GetCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Get.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}