using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.Associate.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch associate-cart command.
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(AssociateCartWithUser))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Associate.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Associate.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
