namespace Module.Promotions.Domain.Promotions;

public static class PromotionConstant
{
    public static class Defaults
    {
        public const MatchPolicy MatchPolicy = Promotions.MatchPolicy.All;
        public const PromotionKind Kind = PromotionKind.CouponCode;
        public const bool Active = true;
        public const bool Advertise = false;
        public const int Position = 0;
    }

    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 128;
        public const int MaxDescriptionLength = 2000;
        public const int MaxPathLength = 500;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Promotion.Name),
            nameof(Promotion.Code),
            nameof(Promotion.Description)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Promotion.Name),
            nameof(Promotion.Position),
            nameof(Promotion.CreatedAtUtc),
            nameof(Promotion.StartsAtUtc),
            nameof(Promotion.ExpiresAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Promotion.Kind),
            nameof(Promotion.MatchPolicy),
            nameof(Promotion.Active),
            nameof(Promotion.Advertise),
            nameof(Promotion.IsDeleted),
            nameof(Promotion.CreatedAtUtc)
        ];
    }
}