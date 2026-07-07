using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Models;

/// <summary>Abstract base class for promotion update parameters (PATCH semantics). All properties are nullable.</summary>
public abstract class PromotionUpdateParameters
{
    /// <summary>Gets or sets the promotion name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the optional code.</summary>
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

    /// <summary>Gets or sets the optional match policy.</summary>
    public MatchPolicy? MatchPolicy { get; init; }

    /// <summary>Gets or sets the optional promotion kind.</summary>
    public PromotionKind? Kind { get; init; }

    /// <summary>Gets or sets the optional advertise flag.</summary>
    public bool? Advertise { get; init; }

    /// <summary>Gets or sets the optional active flag.</summary>
    public bool? Active { get; init; }

    /// <summary>Gets or sets the optional position.</summary>
    public int? Position { get; init; }

    /// <summary>Gets or sets the optional URL path.</summary>
    public string? Path { get; init; }
}
