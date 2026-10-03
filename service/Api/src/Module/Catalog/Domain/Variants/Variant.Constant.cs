using Shared.Application.Domain.Currencies;

namespace Module.Catalog.Domain.Variants;

/// <summary>
/// Contains constant values for the Variant domain.
/// </summary>
public static class VariantConstant
{
    /// <summary>
    /// Default values for variant properties.
    /// </summary>
    public static class Defaults
    {
        public const bool TrackInventory = true;
        public const int Position = 0;
        public const decimal Price = 0m;
        public const decimal CostPrice = 0m;
        public const string CostCurrency = "USD";

        public const decimal Weight = 0m;

        public const decimal Height = 0m;
        public const decimal Width = 0m;
        public const decimal Depth = 0m;

        public const int MaxOptionValues = 100; // Arbitrary limit to prevent excessive options
    }

    /// <summary>
    /// Validation constraints for variant properties.
    /// </summary>
    public static class Constraints
    {
        public const int SkuMaxLength = 255;
        public const int BarcodeMaxLength = 255;
        public const int HsCodeMaxLength = 20;
        public const decimal MinPrice = 0;
        public const int MinPosition = -1;
        public const int MaxUnitStringLength = 10;

        public static class Price
        {
            public const decimal MinValue = 0;
            public const int Precision = 18;
            public const int Scale = 2;
            public const int CurrencyMaxLength = 3;
            public static readonly string[] AllowedCurrencies = [.. SystemCurrency.Supported.Keys];
        }

        public static class Dimensions
        {
            public const decimal MinValue = 0;
        }

        public static class Weight
        {
            public const decimal MinValue = 0;
            public const int Precision = 18;
            public const int Scale = 2;
        }
    }

    /// <summary>
    /// Field metadata for searching, sorting, and filtering.
    /// </summary>
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Variant.Sku),
            nameof(Variant.Barcode),
            nameof(Variant.HsCode)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Variant.Sku),
            nameof(Variant.Position),
            nameof(Variant.Price),
            nameof(Variant.Weight),
            nameof(Variant.Height),
            nameof(Variant.Width),
            nameof(Variant.Depth)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Variant.IsMaster),
            nameof(Variant.TrackInventory),
            nameof(Variant.IsDeleted),
            nameof(Variant.DiscontinuedOn),
            nameof(Variant.DimensionsUnit),
            nameof(Variant.WeightUnit)
        ];
    }
}