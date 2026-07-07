namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

/// <summary>Detail response for a promotion category.</summary>
public class PromotionCategoryDetailResponse : PromotionCategoryParameters
{
    /// <summary>Gets or sets the category ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the last modification timestamp.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

/// <summary>List item response for a promotion category.</summary>
public class PromotionCategoryListItemResponse : PromotionCategoryDetailResponse { }
