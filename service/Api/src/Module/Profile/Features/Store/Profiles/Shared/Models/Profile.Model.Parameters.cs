namespace Module.Profile.Features.Store.Profiles.Shared.Models;

public abstract class ProfileParameter
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTimeOffset? DateOfBirth { get; init; }

    public ProfilePreferences? Preferences { get; init; }
    public ProfileNotificationPreferences? Notifications { get; init; }
}