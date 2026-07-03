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
        public static readonly string[] AllowedSearchFields =
        [
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation),
            nameof(OptionValue.Position)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(OptionValue.Name),
            nameof(OptionValue.Presentation),
            nameof(OptionValue.OptionTypeId)
        ];
    }
}