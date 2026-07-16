using Module.Profile.Features.Store.Profiles.Shared.Models;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Response : ProfileNotificationPreferences;
}