namespace Module.Profile.Features.Store.NotificationPreferences.Get;

public static partial class GetNotificationPreferences
{
    public sealed class Response
    {
        public bool EnableSms { get; init; }
        public bool EnableEmail { get; init; }
        public bool EnableNewsfeeds { get; init; }
    }
}