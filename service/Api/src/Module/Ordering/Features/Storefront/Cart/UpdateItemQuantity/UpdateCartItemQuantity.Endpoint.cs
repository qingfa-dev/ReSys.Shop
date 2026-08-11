using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    /// <summary>Maps the storefront cart item quantity update route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/storefront/cart/items/{lineItemId:guid} — update a cart line item quantity
            app.MapPatch(OrderingFeature.Storefront.Cart.UpdateItemQuantity.Route, async (
                [FromRoute] Guid lineItemId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(lineItemId, request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(UpdateCartItemQuantity))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.UpdateItemQuantity.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.UpdateItemQuantity.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}