using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Profiles.Delete;

public static partial class DeleteProfile
{
    public sealed class Endpoint : ICarterModule
    {
        // Route: DELETE api/store/identity/profiles — deactivate authenticated user's profile
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Store.Profiles.Delete.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                if (!Guid.TryParse(currentUser.UserId, out var userId))
                    return Results.Unauthorized();
                var command = new Command(userId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Store.Profiles.Delete.Summary)
            .WithDescription(ProfileFeature.Store.Profiles.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}