namespace Module.Location.Domain.States;

public static class StateConstant
{
    // Constraints
    public static class Constraints
    {
        public const int MaxNameLength = 100;
        public const int MaxAbbreviationLength = 10;
        public const int MaxCountryIdLength = 36;
        public const int MaxCreatedByLength = 100;
        public const int MaxModifiedByLength = 100;

        // Query Constraints
        public const int MinPage = 1;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int MaxSearchLength = 100;
        public const int MaxFilterLength = 500;
    }

    // Default Values
    public static class Defaults
    {
        public const bool IsActive = true;
    }

    // Ransack: Allowed searchable attributes for state list queries
    public static readonly string[] AllowedSearchFields =
    [
        nameof(State.Name),
        nameof(State.Abbreviation),
        nameof(State.CountryId),
        nameof(State.CreatedBy),
        nameof(State.ModifiedBy)
    ];

    // Sort: Allowed sort fields for state list queries
    public static readonly string[] AllowedSortFields =
    [
        nameof(State.Name),
        nameof(State.Abbreviation),
        nameof(State.CountryId),
        nameof(State.CreatedAtUtc),
        nameof(State.ModifiedAtUtc),
        nameof(State.IsActive)
    ];

    // Filter: Allowed filter fields for state list queries
    public static readonly string[] AllowedFilterFields =
    [
        nameof(State.Name),
        nameof(State.Abbreviation),
        nameof(State.CountryId),
        nameof(State.IsActive),
        nameof(State.CreatedAtUtc),
        nameof(State.ModifiedAtUtc),
        "Country.Name"
    ];
}