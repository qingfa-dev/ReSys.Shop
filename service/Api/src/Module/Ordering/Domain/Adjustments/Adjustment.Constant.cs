namespace Module.Ordering.Domain.Adjustments;

// Initialize: Default values, constraints, and query configuration for Adjustment entity
public static class AdjustmentConstant
{
    public static class Constraints
    {
        // Guard: Label field length — matches UI input max-length and DB column nvarchar(255)
        public const int MaxLabelLength = 255;
        // Guard: DisplayAmount string buffer — "F2" format of decimal.MaxValue fits within 50 chars
        public const int MaxDisplayAmountLength = 50;
        // Guard: AdjustableType and SourceType discriminator strings — prevents unbounded varchar columns
        public const int MaxTypeStrings = 100;
        // Guard: State string buffer — "open"/"closed" lifecycle values stored as text
        public const int MaxStateLength = 50;
        // Guard: Monetary arithmetic precision — shared with Shared kernel for cross-aggregate consistency
        public const int MonetaryPrecision = 18;
        // Guard: Decimal places for monetary values — matches ISO 4217 minor-unit convention for most currencies
        public const int MonetaryScale = 2;
    }

    public static class Defaults
    {
        // Initialize: New adjustments are eligible by default — opt-out model simplifies the common case (discounts)
        public const bool Eligible = true;
        // Initialize: New adjustments are excluded from total until explicitly opted in (e.g. shipping, tax opt-in)
        public const bool Included = false;
        // Initialize: Adjustments are non-mandatory by default — only regulatory fees override this
        public const bool Mandatory = false;
        // Initialize: New adjustments start in "open" state — only closed adjustments are frozen for recalculation
        public const string State = "open";
        // Initialize: System user identifier for auto-created adjustments (e.g. shipping)
        public const string CreatedBy = "System";
    }

    public static class SourceTypes
    {
        public const string Shipping = "Shipping";
    }

    public static class AdjustableTypes
    {
        public const string Order = "Order";
        public const string LineItem = "LineItem";
        public const string Shipment = "Shipment";
    }

    public static class Labels
    {
        public const string Shipping = "Shipping";
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