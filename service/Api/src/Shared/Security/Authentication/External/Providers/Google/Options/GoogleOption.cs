namespace Shared.Security.Authentication.External.Providers.Google.Options;

/// <summary>
/// Configuration options for Google OAuth login, bound from appsettings.json.
/// </summary>
public class GoogleOptions
{
    /// <summary>Configuration section path bound in appsettings.json.</summary>
    public const string SectionName = "Authentication:Google";
    /// <summary>Google OAuth client ID from Google Cloud Console.</summary>
    public string ClientId { get; set; } = string.Empty;
}
