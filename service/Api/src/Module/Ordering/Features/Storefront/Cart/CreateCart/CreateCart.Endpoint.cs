using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.CreateCart;

public static partial class CreateCart
{
    /// <summary>Maps the storefront cart creation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/cart — create a new shopping cart
            app.MapPost(OrderingFeature.Storefront.Cart.Create.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(CreateCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Create.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}