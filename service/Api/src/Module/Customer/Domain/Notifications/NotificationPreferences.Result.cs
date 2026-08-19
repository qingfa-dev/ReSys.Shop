// Contract: All error factories return Error instances with unique codes for traceability
namespace Module.Customer.Domain.Notifications;

public static class NotificationPreferencesResult
{
    public static class Success
    {
        public const string NotificationsCreated = "Notification preferences created successfully";
        public const string NotificationsUpdated = "Notification preferences updated successfully";
    }

    public static class Failure
    {
        public static Error InvalidPreferences => Error.Validation(
            code: "NotificationPreferences.Invalid",
            message: "Invalid notification preferences");
    }
}