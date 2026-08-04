namespace Module.Profile.Features.Shared.Profiles.Models;

public abstract record ProfileParameters
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public ProfilePreferences? Preferences { get; init; }
    public ProfileNotificationPreferences? Notifications { get; init; }
}

public record ProfilePreferences
{
    public string? PreferredStyle { get; init; }
    public string? PreferredFit { get; init; }
    public List<string> FavoriteColors { get; init; } = [];
    public List<string> FavoriteCategories { get; init; } = [];
    public List<string> PreferredBrands { get; init; } = [];
    public string? SizeTop { get; init; }
    public string? SizeBottom { get; init; }
    public string? ShoeSize { get; init; }
}

public record ProfileNotificationPreferences
{
    public bool EnableSms { get; init; } = true;
    public bool EnableEmail { get; init; } = true;
    public bool EnableNewsfeeds { get; init; } = true;
}
