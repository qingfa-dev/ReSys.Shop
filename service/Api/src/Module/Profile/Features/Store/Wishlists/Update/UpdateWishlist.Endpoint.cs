using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Wishlists.Update;

public static partial class UpdateWishlist
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Store.Wishlists.Update.Route, async (
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
                .WithName(nameof(UpdateWishlist))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Store.Wishlists.Update.Summary)
                .WithDescription(ProfileFeature.Store.Wishlists.Update.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
