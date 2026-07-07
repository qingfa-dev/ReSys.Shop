namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

/// <summary>Abstract base class for promotion category update parameters (PATCH semantics). All properties are nullable.</summary>
public abstract class PromotionCategoryUpdateParameters
{
    /// <summary>Gets or sets the category name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the optional unique code.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional display name.</summary>
    public string? Presentation { get; init; }
}
