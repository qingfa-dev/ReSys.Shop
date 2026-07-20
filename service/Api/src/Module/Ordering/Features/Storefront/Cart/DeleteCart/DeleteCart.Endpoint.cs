using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.DeleteCart;

public static partial class DeleteCart
{
    /// <summary>Maps the storefront cart deletion route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE api/storefront/cart — delete the shopping cart
            app.MapDelete(OrderingFeature.Storefront.Cart.Delete.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(DeleteCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Delete.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}