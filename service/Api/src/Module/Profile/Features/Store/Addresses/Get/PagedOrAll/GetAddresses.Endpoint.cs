using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfilesFeature.Store.Addresses.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, cancellationToken);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetAddresses))
            .WithTags(ProfilesFeature.Tags.Address)
            .WithSummary(ProfilesFeature.Store.Addresses.GetAll.Summary)
            .WithDescription(ProfilesFeature.Store.Addresses.GetAll.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
