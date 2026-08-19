using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Wishlists.GetById;

public static partial class GetWishlistById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Storefront.Wishlists.GetById.Route, async (
                    [FromRoute] Guid id,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var query = new Query(Guid.Parse(currentUser.UserId), id);
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(GetWishlistById))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Storefront.Wishlists.GetById.Summary)
                .WithDescription(ProfileFeature.Storefront.Wishlists.GetById.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
