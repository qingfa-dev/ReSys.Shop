using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Storefront.Addresses.GetDefault.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                var query = new Query(Guid.Parse(currentUser.UserId));
                var result = await sender.Send(query, cancellationToken);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetDefaultAddress))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Storefront.Addresses.GetDefault.Summary)
            .WithDescription(ProfileFeature.Storefront.Addresses.GetDefault.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
