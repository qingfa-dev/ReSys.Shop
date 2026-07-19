using Module.Profile.Features.Admin.Profiles.Shared.Models;

namespace Module.Profile.Features.Store.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed record Response : ProfileNotificationPreferences;
}