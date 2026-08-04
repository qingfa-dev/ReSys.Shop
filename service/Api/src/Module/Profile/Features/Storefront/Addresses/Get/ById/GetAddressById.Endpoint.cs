using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Storefront.Addresses.Get.ById;

public static partial class GetAddressById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Storefront.Addresses.GetById.Route, async (
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
            .WithName(nameof(GetAddressById))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Storefront.Addresses.GetById.Summary)
            .WithDescription(ProfileFeature.Storefront.Addresses.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
