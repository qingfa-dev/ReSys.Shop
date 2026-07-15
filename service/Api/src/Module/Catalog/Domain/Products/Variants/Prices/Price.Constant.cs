using Shared.Application.Domain.Currencies;

namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceConstant
{
    public static class Constraints
    {
        public const int CurrencyMaxLength = SystemCurrencyConstant.Constraints.MaxCodeLength;
        public const int CountryIsoMaxLength = 2;
        public const decimal MinAmount = 0m;
        public const decimal MinCompareAtAmount = 0m;
        public const int Precision = SystemCurrencyConstant.Constraints.MonetaryPrecision;
        public const int Scale = SystemCurrencyConstant.Constraints.MonetaryScale;
        public const int MaxPriceListsPerVariant = 10;
    }

    public static class Default
    {
        public const string Currency = SystemCurrencyConstant.Defaults.Code;
        public const bool IsDefault = false;
    }

    public static class Bulk
    {
        public const decimal PercentageDenominator = 100m;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Price.Currency),
            nameof(Price.CountryIso)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Price.Amount),
            nameof(Price.Currency),
            nameof(Price.CompareAtAmount)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Price.Currency),
            nameof(Price.CountryIso),
            nameof(Price.IsDefault),
            nameof(Price.PriceListId),
            nameof(Price.CompareAtAmount)
        ];
    }
}