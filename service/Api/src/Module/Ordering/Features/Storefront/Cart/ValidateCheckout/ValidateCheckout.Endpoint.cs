using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.ValidateCheckout;

public static partial class ValidateCheckout
{
    /// <summary>Maps the storefront checkout validation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/cart/validate — validate the current checkout state
            app.MapPost(OrderingFeature.Storefront.Cart.Validate.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(ValidateCheckout))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Validate.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Validate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}