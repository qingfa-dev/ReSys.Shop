using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Storefront.NotificationPreferences.Get.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                if (string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new Query(Guid.Parse(currentUser.UserId)), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetNotificationPreferences))
            .WithTags(ProfileFeature.Tags.NotificationPreferences)
            .WithSummary(ProfileFeature.Storefront.NotificationPreferences.Get.Summary)
            .WithDescription(ProfileFeature.Storefront.NotificationPreferences.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
