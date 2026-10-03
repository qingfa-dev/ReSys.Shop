using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    /// <summary>Maps the storefront add-to-cart route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/cart/items — add an item to the shopping cart
            app.MapPost(OrderingFeature.Storefront.Cart.AddItem.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(AddToCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.AddItem.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.AddItem.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}