using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

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
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var command = new Command(Guid.Parse(currentUser.UserId), id, request);
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
