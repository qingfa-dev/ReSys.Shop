using Module.Profile.Features.Admin.Profiles.Shared.Models;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Request : ProfileNotificationPreferences;
}
