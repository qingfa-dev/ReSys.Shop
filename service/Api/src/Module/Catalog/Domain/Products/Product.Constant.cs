namespace Module.Catalog.Domain.Products;

/// <summary>
/// Contains constant values for the Product domain.
/// </summary>
public static class ProductConstant
{
    /// <summary>
    /// Default values for product properties.
    /// </summary>
    public static class Defaults
    {
    }

    /// <summary>
    /// Validation constraints for product properties.
    /// </summary>
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxDescriptionLength = 2000;
        public const int MaxSlugLength = 255;
        public const int MaxMetaTitleLength = 100;
        public const int MaxMetaDescriptionLength = 255;
        public const int MaxMetaKeywordsLength = 255;

        public const int MaxStyleCodeLength = 50;
        public const int MaxSeasonNameLength = 50;
        public const int MaxMaterialCompositionLength = 500;
        public const int MaxCareInstructionsLength = 500;
        public const int MaxFitNotesLength = 500;
        public const int MaxDepartmentLength = 50;
        public const int MaxGenderTargetLength = 20;
    }

    /// <summary>
    /// Field metadata for searching, sorting, and filtering.
    /// </summary>
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Product.Name),
            nameof(Product.Description),
            nameof(Product.Slug),
            nameof(Product.StyleCode),
            nameof(Product.SeasonName),
            nameof(Product.Department),
            nameof(Product.GenderTarget)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Product.Name),
            nameof(Product.CreatedAtUtc),
            nameof(Product.ModifiedAtUtc),
            nameof(Product.AvailableOn)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Product.Status),
            nameof(Product.IsDeleted),
            nameof(Product.CreatedAtUtc),
            nameof(Product.AvailableOn),
            nameof(Product.StyleCode),
            nameof(Product.SeasonName),
            nameof(Product.Department),
            nameof(Product.AvailableOn)
        ];
    }
}