using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Profile.Delete;

public static partial class DeleteProfile
{
    public sealed class Endpoint : ICarterModule
    {
        // Route: DELETE api/store/identity/profiles — deactivate authenticated user's profile
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfilesFeature.Store.Profile.Delete.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                var userId = Guid.Parse(currentUser.UserId!);
                var command = new Command(userId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteProfile))
            .WithTags(ProfilesFeature.Tags.Profile)
            .WithSummary(ProfilesFeature.Store.Profile.Delete.Summary)
            .WithDescription(ProfilesFeature.Store.Profile.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
