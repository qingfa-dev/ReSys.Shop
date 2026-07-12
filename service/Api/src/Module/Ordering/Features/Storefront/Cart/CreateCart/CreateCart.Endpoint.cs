using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.CreateCart;

public static partial class CreateCart
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.Create.Route, async (ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch create-cart command.
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(CreateCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Create.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Create.Description)
            .Produces<Result<Response>>();
        }
    }
}
