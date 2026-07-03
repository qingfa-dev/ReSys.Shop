namespace Module.Catalog.Domain.OptionTypes;

public static class OptionTypeConstant
{
    public static class Constraints
    {
        public const int NameMaxLength = 100;
        public const int PresentationMaxLength = 100;
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
            nameof(OptionType.Name),
            nameof(OptionType.Presentation),
            nameof(OptionType.CreatedBy),
            nameof(OptionType.ModifiedBy)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(OptionType.Name),
            nameof(OptionType.Presentation),
            nameof(OptionType.Filterable),
            nameof(OptionType.CreatedBy),
            nameof(OptionType.ModifiedBy)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(OptionType.Name),
            nameof(OptionType.Presentation),
            nameof(OptionType.Filterable),
            nameof(OptionType.CreatedBy),
            nameof(OptionType.ModifiedBy)
        ];
    }
}