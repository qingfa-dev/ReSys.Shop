namespace Module.Catalog.Domain.Taxonomies.Taxons;

public static class TaxonConstant
{
    public static class Constraints
    {
        public const int NameMaxLength = 255;
        public const int PresentationMaxLength = 255;
        public const int SlugMaxLength = 255;
        public const int PermalinkMaxLength = 500;
        public const int PrettyNameMaxLength = 500;
        public const int DescriptionMaxLength = 2000;
        public const int DescriptionHtmlMaxLength = 4000;
        public const int MetaTitleMaxLength = 255;
        public const int MetaDescriptionMaxLength = 1000;
        public const int MetaKeywordsMaxLength = 255;
        public const int UrlMaxLength = 2000;
        public const int PolicyMaxLength = 50;
        public const int SortOrderMaxLength = 50;
        public const int MinPosition = -1;
    }

    public static class Default
    {
        public const int Position = 0;
    }

    public static class Patterns
    {
        public const string Slug = @"^[a-z0-9]+(?:-[a-z0-9]+)*$";
        public const string Url = @"^(https?://)?([\w\-]+\.)+[\w\-]+(/[\w\-./?%&=]*)?$";
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Taxon.Name),
            nameof(Taxon.Presentation),
            nameof(Taxon.Slug)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Taxon.Name),
            nameof(Taxon.Presentation),
            nameof(Taxon.Position),
            nameof(Taxon.Depth)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Taxon.Name),
            nameof(Taxon.Presentation),
            nameof(Taxon.Slug),
            nameof(Taxon.HideFromNav),
            nameof(Taxon.Lft),
            nameof(Taxon.Rgt),
            nameof(Taxon.Depth)
        ];
    }
}