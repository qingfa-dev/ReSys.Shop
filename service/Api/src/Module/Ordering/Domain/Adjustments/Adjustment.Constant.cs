namespace Module.Ordering.Domain.Adjustments;

// Initialize: Default values, constraints, and query configuration for Adjustment entity
public static class AdjustmentConstant
{
    public static class Constraints
    {
        public const int MaxLabelLength = 255;
        public const int MaxDisplayAmountLength = 50;
        public const int MaxTypeStrings = 100;
        public const int MonetaryPrecision = 18;
        public const int MonetaryScale = 2;
    }

    public static class Defaults
    {
        public const bool Eligible = true;
        public const bool Included = false;
        public const bool Mandatory = false;
        public const string State = "open";
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Adjustment.Label)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Adjustment.Amount),
            nameof(Adjustment.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Adjustment.Eligible),
            nameof(Adjustment.Included),
            nameof(Adjustment.Mandatory),
            nameof(Adjustment.State),
            nameof(Adjustment.OrderId),
            nameof(Adjustment.AdjustableType),
            nameof(Adjustment.SourceType)
        ];
    }
}
