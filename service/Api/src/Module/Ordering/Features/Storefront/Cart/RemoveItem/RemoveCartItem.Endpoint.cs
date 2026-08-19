using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

public static partial class RemoveCartItem
{
    /// <summary>Maps the storefront cart item removal route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE api/storefront/cart/items/{lineItemId:guid} — remove a line item from the cart
            app.MapDelete(OrderingFeature.Storefront.Cart.RemoveItem.Route, async (
                [FromRoute] Guid lineItemId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(lineItemId), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(RemoveCartItem))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.RemoveItem.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.RemoveItem.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}