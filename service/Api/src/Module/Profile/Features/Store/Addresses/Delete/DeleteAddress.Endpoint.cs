using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Addresses.Delete;

public static partial class DeleteAddress
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfilesFeature.Store.Addresses.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, cancellationToken);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteAddress))
            .WithTags(ProfilesFeature.Tags.Address)
            .WithSummary(ProfilesFeature.Store.Addresses.Delete.Summary)
            .WithDescription(ProfilesFeature.Store.Addresses.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
