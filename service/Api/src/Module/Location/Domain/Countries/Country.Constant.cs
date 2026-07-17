namespace Module.Location.Domain.Countries;

/// <summary>Contains default values, patterns, and query configuration for Country entities.</summary>
public static class CountryConstant
{
    // Constraints
    public static class Constraints
    {
        public const int MaxNameLength = 100;
        public const int MaxIsoCodeLength = 3;
        public const int MaxIso3CodeLength = 3;
        public const int MaxIsoNameLength = 100;
        public const int MaxCallingCodeLength = 10;

        // Import Constraints
        public const int MaxImportBatchSize = 1000;
    }

    // Query Constraints
    public static class Query
    {
        public const int MinPage = 1;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int MaxSearchLength = 100;
        public const int MaxFilterLength = 500;

        // Ransack: Allowed searchable attributes for country list queries
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Country.Name),
            nameof(Country.IsoCode),
            nameof(Country.Iso3Code),
            nameof(Country.IsoName),
            nameof(Country.CallingCode),
            nameof(Country.CreatedBy),
            nameof(Country.ModifiedBy)
        ];

        // Sort: Allowed sort fields for country list queries
        public static readonly string[] AllowedSortFields =
        [
            nameof(Country.Name),
            nameof(Country.IsoCode),
            nameof(Country.Iso3Code),
            nameof(Country.IsoName),
            nameof(Country.CreatedAtUtc),
            nameof(Country.ModifiedAtUtc),
            nameof(Country.IsActive),
            nameof(Country.StatesRequired)
        ];

        // Filter: Allowed filter fields for country list queries
        public static readonly string[] AllowedFilterFields =
        [
            nameof(Country.Name),
            nameof(Country.IsoCode),
            nameof(Country.Iso3Code),
            nameof(Country.IsoName),
            nameof(Country.CallingCode),
            nameof(Country.IsActive),
            nameof(Country.StatesRequired),
            nameof(Country.CreatedAtUtc),
            nameof(Country.ModifiedAtUtc),
            "States.Count"
        ];
    }

    // Context: Country default values reference UN/LOCODE and ISO 3166-1 maintenance agency recommendations
    // Default Values
    /// <summary>Default configuration values for Country entity properties.</summary>
    public static class Defaults
    {
        public const bool StatesRequired = false;
        public const bool IsActive = true;
    }

}