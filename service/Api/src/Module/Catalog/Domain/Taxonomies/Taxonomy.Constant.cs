namespace Module.Catalog.Domain.Taxonomies;

public static class TaxonomyConstant
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
            nameof(Taxonomy.Name),
            nameof(Taxonomy.Presentation),
            nameof(Taxonomy.CreatedBy),
            nameof(Taxonomy.ModifiedBy)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Taxonomy.Name),
            nameof(Taxonomy.Presentation),
            nameof(Taxonomy.CreatedBy),
            nameof(Taxonomy.ModifiedBy)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Taxonomy.Name),
            nameof(Taxonomy.Presentation),
            nameof(Taxonomy.CreatedBy),
            nameof(Taxonomy.ModifiedBy)
        ];
    }
}