namespace Shared.Security.Authentication.External.Providers.Facebook.Options;

public class FacebookOptions
{
    public const string SectionName = "Authentication:Facebook";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}
