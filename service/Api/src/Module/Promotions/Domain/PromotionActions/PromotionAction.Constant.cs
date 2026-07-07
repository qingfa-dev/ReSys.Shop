namespace Module.Promotions.Domain.PromotionActions;

public static class PromotionActionConstant
{
    public static class Constraints
    {
        public const int MaxTypeLength = 100;
        public const int MaxCalculatorTypeLength = 100;
    }

    public static class Types
    {
        public const string CreateAdjustment = "CreateAdjustment";
        public const string FreeShipping = "FreeShipping";
        public const string CreateLineItems = "CreateLineItems";
        public const string CreatePriceAdjustment = "CreatePriceAdjustment";
        public const string CreateItemAdjustment = "CreateItemAdjustment";

        public static readonly string[] All =
        [
            CreateAdjustment, FreeShipping, CreateLineItems, CreatePriceAdjustment, CreateItemAdjustment
        ];
    }

    public static class CalculatorTypes
    {
        public const string FlatRate = "FlatRate";
        public const string Percent = "Percent";
        public const string TieredPercent = "TieredPercent";
        public const string TieredFlatRate = "TieredFlatRate";
        public const string FlexibleRate = "FlexibleRate";
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PromotionAction.Type),
            nameof(PromotionAction.CalculatorType)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PromotionAction.Type),
            nameof(PromotionAction.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PromotionAction.Type),
            nameof(PromotionAction.CalculatorType),
            nameof(PromotionAction.PromotionId),
            nameof(PromotionAction.CreatedAtUtc)
        ];
    }
}