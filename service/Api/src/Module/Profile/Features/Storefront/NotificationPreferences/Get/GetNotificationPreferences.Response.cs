using Module.Profile.Features.Shared.Profiles.Models;

namespace Module.Profile.Features.Storefront.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed record Response : ProfileNotificationPreferences;
}