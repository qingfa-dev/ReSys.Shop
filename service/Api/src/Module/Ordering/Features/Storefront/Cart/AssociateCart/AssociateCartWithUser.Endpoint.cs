using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    /// <summary>Maps the storefront cart association route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/cart/associate — associate a guest cart with the current user
            app.MapPost(OrderingFeature.Storefront.Cart.Associate.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(AssociateCartWithUser))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Associate.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Associate.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}