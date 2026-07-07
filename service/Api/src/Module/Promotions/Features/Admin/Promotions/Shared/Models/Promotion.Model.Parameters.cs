using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Models;

/// <summary>Abstract base class for promotion-related parameters.</summary>
public abstract class PromotionParameters
{
    /// <summary>Gets or sets the promotion name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional promotion code.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the optional usage limit.</summary>
    public int? UsageLimit { get; init; }

    /// <summary>Gets or sets the optional per-customer usage limit.</summary>
    public int? PerCustomerUsageLimit { get; init; }

    /// <summary>Gets or sets the optional start date.</summary>
    public DateTimeOffset? StartsAtUtc { get; init; }

    /// <summary>Gets or sets the optional expiration date.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    /// <summary>Gets or sets the match policy.</summary>
    public MatchPolicy MatchPolicy { get; init; } = PromotionConstant.Defaults.MatchPolicy;

    /// <summary>Gets or sets the promotion kind.</summary>
    public PromotionKind Kind { get; init; } = PromotionConstant.Defaults.Kind;

    /// <summary>Gets or sets whether to advertise the promotion.</summary>
    public bool Advertise { get; init; }

    /// <summary>Gets or sets whether the promotion is active.</summary>
    public bool Active { get; init; } = PromotionConstant.Defaults.Active;

    /// <summary>Gets or sets the display position.</summary>
    public int Position { get; init; }

    /// <summary>Gets or sets the optional URL path.</summary>
    public string? Path { get; init; }
}
