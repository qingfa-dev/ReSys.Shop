using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.ValidateCheckout;

public static partial class ValidateCheckout
{
    /// <summary>Maps the storefront checkout validation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/cart/checkout — validate the current checkout state
            app.MapGet(OrderingFeature.Storefront.Cart.ValidateCheckout.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(ValidateCheckout))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.ValidateCheckout.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.ValidateCheckout.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}