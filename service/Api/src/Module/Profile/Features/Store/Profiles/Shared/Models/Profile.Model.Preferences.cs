namespace Module.Profile.Features.Store.Profiles.Shared.Models;

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

    public static readonly ProfilePreferences Empty = new();
}