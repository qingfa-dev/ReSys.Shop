namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed record Response(bool EnableSms, bool EnableEmail, bool EnableNewsfeeds);
}