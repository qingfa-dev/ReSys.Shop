namespace Module.Promotions.Domain.PromotionCategories;

public static class PromotionCategoryConstant
{
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 128;
        public const int MaxPresentationLength = 255;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PromotionCategory.Name),
            nameof(PromotionCategory.Code)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PromotionCategory.Name),
            nameof(PromotionCategory.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PromotionCategory.IsDeleted),
            nameof(PromotionCategory.CreatedAtUtc)
        ];
    }
}