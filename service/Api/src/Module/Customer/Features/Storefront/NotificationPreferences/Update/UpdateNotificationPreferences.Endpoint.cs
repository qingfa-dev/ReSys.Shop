using Module.Customer.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Features.Storefront.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(ProfileFeature.Storefront.NotificationPreferences.Update.Route, async (
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
            .WithSummary(ProfileFeature.Storefront.NotificationPreferences.Update.Summary)
            .WithDescription(ProfileFeature.Storefront.NotificationPreferences.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
