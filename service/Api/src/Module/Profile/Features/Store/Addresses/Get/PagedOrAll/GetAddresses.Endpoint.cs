using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Store.Addresses.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                var query = new Query(parameters with { UserId = Guid.Parse(currentUser.UserId) });
                var result = await sender.Send(query, cancellationToken);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetAddresses))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Store.Addresses.GetAll.Summary)
            .WithDescription(ProfileFeature.Store.Addresses.GetAll.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
