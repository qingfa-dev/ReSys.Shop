using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.EmptyCart;

public static partial class EmptyCart
{
    /// <summary>Maps the storefront empty-cart route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE api/storefront/cart/items — remove all items from the cart
            app.MapDelete(OrderingFeature.Storefront.Cart.RemoveAllItems.Route,
                async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(EmptyCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.RemoveAllItems.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.RemoveAllItems.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}