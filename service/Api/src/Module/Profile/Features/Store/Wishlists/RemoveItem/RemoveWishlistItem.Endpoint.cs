using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.RemoveItem;

public static partial class RemoveWishlistItem
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Store.Wishlists.RemoveItem.Route, async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid itemId,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var command = new Command(Guid.Parse(currentUser.UserId), id, itemId);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(RemoveWishlistItem))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Store.Wishlists.RemoveItem.Summary)
                .WithDescription(ProfileFeature.Store.Wishlists.RemoveItem.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
