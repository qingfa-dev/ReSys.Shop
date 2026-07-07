namespace Module.Promotions.Domain.Promotions;

// AgentHint: Ransackable attributes define which fields are searchable via query filters;
//            mirror Ruby whitelisted_ransackable_attributes and associations
public sealed partial class Promotion
{
    #region Ransackable Attributes
    /// <summary>Gets the list of attribute names allowed in search/filter queries.</summary>
    public static readonly string[] RansackableAttributes =
    [
        nameof(Path),
        nameof(Code),
        nameof(Name),
        nameof(Description),
        nameof(StartsAtUtc),
        nameof(ExpiresAtUtc),
        nameof(Kind),
        nameof(MatchPolicy),
        nameof(Active),
        nameof(Advertise)
    ];

    /// <summary>Gets the list of navigation property names allowed in search/filter queries.</summary>
    public static readonly string[] RansackableAssociations =
    [
        "CouponCodes",
        "PromotionRules",
        "PromotionActions",
        "Stores"
    ];
    #endregion Ransackable Attributes
}