using BuildingBlocks.Calculators;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Promotions Calculator Result.</summary>

public static class PromotionsCalculatorResult
{
    public static class Success
    {
        public const string FlatComputed = "Flat rate computed successfully.";
        public const string PercentComputed = "Percent-based discount computed successfully.";
        public const string TieredComputed = "Tiered rate computed successfully.";
    }

    public static class Errors
    {
        public static Error InvalidCurrency(string currency) => Error.Validation(
            code: "PromotionsCalculator.InvalidCurrency",
            description: $"Currency '{currency}' is not supported by this calculator.");

        public static Error PercentOutOfRange(decimal percent) => Error.Validation(
            code: "PromotionsCalculator.PercentOutOfRange",
            description: $"Percent '{percent}' exceeds maximum of {PromotionsCalculatorConstant.Constraints.MaxPercent}.",
            field: "Percent");

        public static Error InvalidTierConfiguration => Error.Validation(
            code: "PromotionsCalculator.InvalidTierConfiguration",
            description: "Tier configuration is invalid or empty.");

        public static Error FullPricedItemGuard => Error.Validation(
            code: "PromotionsCalculator.FullPricedItemGuard",
            description: "Calculator applies only to full-priced items.");

        public static Error NotApplicable => Error.Validation(
            code: "PromotionsCalculator.NotApplicable",
            description: CalculatorConstant.Defaults.NotApplicableMessage);
    }
}