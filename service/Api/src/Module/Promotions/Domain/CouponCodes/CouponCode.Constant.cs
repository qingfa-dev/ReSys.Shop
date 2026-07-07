namespace Module.Promotions.Domain.CouponCodes;

public static class CouponCodeConstant
{
    public static class Defaults
    {
        public const CouponCodeState State = CouponCodeState.Active;
    }

    public static class Constraints
    {
        public const int MaxCodeLength = 128;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(CouponCode.Code)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(CouponCode.Code),
            nameof(CouponCode.CreatedAtUtc),
            nameof(CouponCode.RedeemedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(CouponCode.State),
            nameof(CouponCode.PromotionId),
            nameof(CouponCode.OrderId),
            nameof(CouponCode.CreatedAtUtc)
        ];
    }
}