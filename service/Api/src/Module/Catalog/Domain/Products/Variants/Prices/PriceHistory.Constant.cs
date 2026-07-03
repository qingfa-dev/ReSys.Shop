namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceHistoryConstant
{
    public static class Constraints
    {
        public const int CurrencyMaxLength = 3;
        public const decimal MinAmount = 0m;
        public const int DefaultRecentDays = 30;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PriceHistory.Currency),
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PriceHistory.Amount),
            nameof(PriceHistory.RecordedAt),
            nameof(PriceHistory.Currency),
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PriceHistory.Currency),
            nameof(PriceHistory.VariantId),
            nameof(PriceHistory.PriceId),
            nameof(PriceHistory.RecordedAt),
        ];
    }
}
