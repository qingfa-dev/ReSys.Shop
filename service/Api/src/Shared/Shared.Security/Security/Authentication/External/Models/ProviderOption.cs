namespace Shared.Security.Authentication.External.Models;

public record ProviderOption
{
    public string Provider { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
}
