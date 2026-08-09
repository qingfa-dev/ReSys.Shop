using Module.Customer.Features.Shared.Profiles.Models;

namespace Module.Customer.Features.Storefront.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed record Response : ProfileNotificationPreferences;
}