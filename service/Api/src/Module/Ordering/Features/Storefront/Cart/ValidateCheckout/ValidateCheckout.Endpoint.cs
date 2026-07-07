using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.ValidateCheckout;

public static partial class ValidateCheckout
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.Validate.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ValidateCheckout))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Validate.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Validate.Description)
            .Produces<Result>();
        }
    }
}
