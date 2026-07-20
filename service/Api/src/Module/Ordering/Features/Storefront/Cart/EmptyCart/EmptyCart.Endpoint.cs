using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.EmptyCart;

public static partial class EmptyCart
{
    /// <summary>Maps the storefront empty-cart route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/cart/empty — remove all items from the cart
            app.MapPost(OrderingFeature.Storefront.Cart.Empty.Route,
                async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(EmptyCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Empty.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Empty.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}