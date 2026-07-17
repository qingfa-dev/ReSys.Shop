namespace Module.Catalog.Domain.OptionTypes.Values;

public static class OptionValueConstant
{
    public static class Constraints
    {
        public const int NameMaxLength = 100;
        public const int PresentationMaxLength = 100;

        // Which always on top
        public const int MinPosition = -1;
    }

    public static class Default
    {
        public const int Position = 1;
    }

    public static class Query
    {
        public static IReadOnlySet<string> AllowedSearchFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation)
        };

        public static IReadOnlySet<string> AllowedSortFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation),
            nameof(OptionValue.Position)
        };

        public static IReadOnlySet<string> AllowedFilterFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation),
            nameof(OptionValue.OptionTypeId)
        };
    }
}