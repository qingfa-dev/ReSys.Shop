namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

/// <summary>Abstract base class for promotion category-related parameters.</summary>
public abstract class PromotionCategoryParameters
{
    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional unique code.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional display name.</summary>
    public string? Presentation { get; init; }
}
