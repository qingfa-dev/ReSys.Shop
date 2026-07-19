using Shared.Application.Domain.Models;

namespace Module.Profile.Domain.Notifications;

/// <summary>Represents notification preferences for a user.</summary>
public sealed partial class NotificationPreferences : ValueObject
{
    public bool EnableSms { get; set; }
    public bool EnableEmail { get; set; }
    public bool EnableNewsfeeds { get; set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EnableSms;
        yield return EnableEmail;
        yield return EnableNewsfeeds;
    }

    public static NotificationPreferences Default => new()
    {
        EnableSms = true,
        EnableEmail = true,
        EnableNewsfeeds = true
    };

    public static Result<NotificationPreferences> Create(
        bool? enableSms = true,
        bool? enableEmail = true,
        bool? enableNewsfeeds = true)
    {
        return NotificationPreferencesMethod.Create(enableSms, enableEmail, enableNewsfeeds);
    }
}