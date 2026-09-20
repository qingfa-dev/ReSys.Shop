namespace Shared.Security.Authentication.External.Providers.Microsoft.Options;

public class MicrosoftOptions
{
    public const string SectionName = "Authentication:Microsoft";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}
