using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Wishlists.Get;

public static partial class GetWishlists
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Storefront.Wishlists.GetAll.Route, async (
                    [AsParameters] Parameters parameters,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var query = new Query(Guid.Parse(currentUser.UserId), parameters);
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToPagedResult();
                })
                .RequireAuthorization()
                .WithName(nameof(GetWishlists))
                .WithTags(ProfileFeature.Tags.Wishlist)
                .WithSummary(ProfileFeature.Storefront.Wishlists.GetAll.Summary)
                .WithDescription(ProfileFeature.Storefront.Wishlists.GetAll.Description)
                .Produces<PagedResult<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
