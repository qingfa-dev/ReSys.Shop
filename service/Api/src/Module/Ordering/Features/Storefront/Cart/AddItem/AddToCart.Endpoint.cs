using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.AddItem.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                // Call: Dispatch add-to-cart command.
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(AddToCart))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.AddItem.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.AddItem.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
