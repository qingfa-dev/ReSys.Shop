namespace Module.Profile.Features.Store.Profiles.Shared.Models;

public record ProfileNotificationPreferences
{
    public bool EnableSms { get; init; } = true;
    public bool EnableEmail { get; init; } = true;
    public bool EnableNewsfeeds { get; init; } = true;

    public static readonly ProfileNotificationPreferences Default = new();
}