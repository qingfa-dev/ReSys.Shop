using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

public static partial class AddWishlistItem
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Store.Wishlists.AddItem.Route, async (
                    [FromRoute] Guid id,
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new Command(id, request);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(AddWishlistItem))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Store.Wishlists.AddItem.Summary)
                .WithDescription(ProfileFeature.Store.Wishlists.AddItem.Description)
                .Produces<Result<Response>>(StatusCodes.Status201Created)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
