using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

public static partial class RemoveCartItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(OrderingFeature.Storefront.Cart.RemoveItem.Route, async (
                Guid lineItemId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(lineItemId), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(RemoveCartItem))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.RemoveItem.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.RemoveItem.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
