using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Addresses.Get.ById;

public static partial class GetAddressById
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Store.Addresses.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, cancellationToken);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetAddressById))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Store.Addresses.GetById.Summary)
            .WithDescription(ProfileFeature.Store.Addresses.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
