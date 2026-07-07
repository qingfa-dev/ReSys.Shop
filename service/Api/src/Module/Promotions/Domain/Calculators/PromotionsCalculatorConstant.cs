namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Promotions Calculator Constant.</summary>

public static class PromotionsCalculatorConstant
{
    public static class Defaults
    {
        public const decimal FlatPercent = 0m;
        public const string Currency = "USD";
        public const int MaxItems = 0;
        public const bool ApplyOnlyOnFullPricedItems = false;
        public const decimal FirstItem = 0m;
        public const decimal AdditionalItem = 0m;
        public const decimal NormalAmount = 0m;
        public const decimal DiscountAmount = 0m;
        public const decimal MinimalAmount = 0m;
        public const decimal BaseAmount = 0m;
        public const decimal BasePercent = 0m;
    }

    public static class Constraints
    {
        public const decimal MaxPercent = 100m;
        public const int MaxTierCount = 50;
    }
}