namespace Module.Promotions.Features.Admin.Promotions.Shared.Models;

/// <summary>Detail response for a promotion.</summary>
public class PromotionDetailResponse : PromotionParameters
{
    /// <summary>Gets or sets the promotion ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the last modification timestamp.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }

    /// <summary>Gets or sets the deletion timestamp.</summary>
    public DateTimeOffset? DeletedAtUtc { get; set; }

    /// <summary>Gets or sets whether the promotion is soft-deleted.</summary>
    public bool IsDeleted { get; set; }
}

/// <summary>List item response for a promotion.</summary>
public class PromotionListItemResponse : PromotionDetailResponse { }
