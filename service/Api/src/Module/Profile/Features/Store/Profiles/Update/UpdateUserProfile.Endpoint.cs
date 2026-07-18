using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Profiles.Update;

public static partial class UpdateUserProfile
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Store.Profile.Update.Route, async (
                [FromBody] Request request,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                if (!Guid.TryParse(currentUser.UserId, out var userId))
                    return Results.Unauthorized();
                var command = new Command(userId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Store.Profile.Update.Summary)
            .WithDescription(ProfileFeature.Store.Profile.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
