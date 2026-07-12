using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Storefront.Cart.UpdateItemQuantity.Route, async (
                Guid lineItemId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
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
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
