using Module.Profile.Features.Shared.Profiles.Models;

namespace Module.Profile.Features.Storefront.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Request : ProfileNotificationPreferences;
}
