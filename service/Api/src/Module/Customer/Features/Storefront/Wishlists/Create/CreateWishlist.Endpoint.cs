using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Wishlists.Create;

public static partial class CreateWishlist
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Storefront.Wishlists.Create.Route, async (
                    [FromBody] Request request,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var command = new Command(Guid.Parse(currentUser.UserId), request);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(CreateWishlist))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Storefront.Wishlists.Create.Summary)
                .WithDescription(ProfileFeature.Storefront.Wishlists.Create.Description)
                .Produces<Result<Response>>(StatusCodes.Status201Created)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
