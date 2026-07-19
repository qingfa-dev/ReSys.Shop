using Module.Profile.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Store.NotificationPreferences.Update.Route, async (
                [FromBody] Request request,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                if (string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new Command(Guid.Parse(currentUser.UserId), request), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(UpdateNotificationPreferences))
            .WithTags(ProfileFeature.Tags.NotificationPreferences)
            .WithSummary(ProfileFeature.Store.NotificationPreferences.Update.Summary)
            .WithDescription(ProfileFeature.Store.NotificationPreferences.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
