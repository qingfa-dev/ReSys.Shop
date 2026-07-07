using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Store.NotificationPreferences.Get.Route, async (
                ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetNotificationPreferences))
            .WithTags(ProfileFeature.Tags.NotificationPreferences)
            .WithSummary(ProfileFeature.Store.NotificationPreferences.Get.Summary)
            .WithDescription(ProfileFeature.Store.NotificationPreferences.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
