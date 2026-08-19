// Invariant: Collection properties (FavoriteColors, FavoriteCategories, PreferredBrands)
//             are non-null; size fields match SizePattern regex

using Shared.Application.Domain.Models;

namespace Module.Customer.Domain.Preferences;

/// <summary>Represents personal preferences for a user such as style, fit, and sizes.</summary>
public sealed partial class UserPreferences : ValueObject
{
    public string? PreferredStyle { get; set; }
    public string? PreferredFit { get; set; }
    public List<string> FavoriteColors { get; set; } = [];
    public List<string> FavoriteCategories { get; set; } = [];
    public List<string> PreferredBrands { get; set; } = [];
    public string? SizeTop { get; set; }
    public string? SizeBottom { get; set; }
    public string? ShoeSize { get; set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PreferredStyle;
        yield return PreferredFit;
        yield return FavoriteColors;
        yield return FavoriteCategories;
        yield return PreferredBrands;
        yield return SizeTop;
        yield return SizeBottom;
        yield return ShoeSize;
    }

    public static UserPreferences Empty => new();

    public static Result<UserPreferences> Create(
        string? preferredStyle = null,
        string? preferredFit = null,
        List<string>? favoriteColors = null,
        List<string>? favoriteCategories = null,
        List<string>? preferredBrands = null,
        string? sizeTop = null,
        string? sizeBottom = null,
        string? shoeSize = null)
    {
        return UserPreferenceMethod.Create(
            preferredStyle,
            preferredFit,
            favoriteColors,
            favoriteCategories,
            preferredBrands,
            sizeTop,
            sizeBottom,
            shoeSize);
    }
}