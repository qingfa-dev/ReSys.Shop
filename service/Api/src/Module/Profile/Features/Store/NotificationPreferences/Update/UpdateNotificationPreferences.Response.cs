namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed class Response
    {
        public bool EnableSms { get; init; }
        public bool EnableEmail { get; init; }
        public bool EnableNewsfeeds { get; init; }
    }
}
