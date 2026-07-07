using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.EmptyCart;

public static partial class EmptyCart
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.Empty.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(EmptyCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Empty.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Empty.Description)
            .Produces<Result>();
        }
    }
}
