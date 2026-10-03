using Module.Customer.Features.Shared.Profiles.Models;

namespace Module.Customer.Features.Storefront.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Request : ProfileNotificationPreferences;
}
