using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.Delete;

public static partial class DeleteWishlist
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Store.Wishlists.Delete.Route, async (
                    [FromRoute] Guid id,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var command = new Command(Guid.Parse(currentUser.UserId), id, DeletedBy: currentUser.UserName);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(DeleteWishlist))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Store.Wishlists.Delete.Summary)
                .WithDescription(ProfileFeature.Store.Wishlists.Delete.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
