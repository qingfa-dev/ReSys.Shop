using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

public static partial class UpdateCheckout
{
    /// <summary>Maps the storefront checkout update route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/storefront/cart — update cart checkout details (email, addresses, instructions)
            app.MapPatch(OrderingFeature.Storefront.Cart.Update.Route, async (
                [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(UpdateCheckout))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Update.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Update.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}