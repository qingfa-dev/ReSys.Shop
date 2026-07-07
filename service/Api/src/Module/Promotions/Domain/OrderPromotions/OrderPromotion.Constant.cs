namespace Module.Promotions.Domain.OrderPromotions;

public static class OrderPromotionConstant
{
    public static class Query
    {
        public static readonly string[] AllowedSearchFields = [];

        public static readonly string[] AllowedSortFields =
        [
            nameof(OrderPromotion.OrderId)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(OrderPromotion.OrderId),
            nameof(OrderPromotion.PromotionId),
            nameof(OrderPromotion.PromotionCodeId)
        ];
    }
}