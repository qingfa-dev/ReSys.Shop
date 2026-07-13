namespace Module.Profile.Features.Store.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed record Response(bool EnableSms, bool EnableEmail, bool EnableNewsfeeds);
}