namespace Shared.Application.Domain.Concerns.Sluggable;

/// <summary>
/// Contains constraints and querying metadata for sluggable entities.
/// </summary>
public static class SluggableConstant
{
    public static class Constraints
    {
        public const int MaxSlugLength = 255;
    }

    public static class Patterns
    {
        public const string Slug = "^[a-z0-9\\-]+$";
    }

    /// <summary>
    /// Contains allowed fields for querying (Search, Sort, Filter).
    /// Note: Typo 'Feilds' is intentional for consistency with StateConstant.Feilds, etc.
    /// </summary>
    public static class Feilds
    {
        public static readonly string[] AllowedSearchFields = ["Slug"];
        public static readonly string[] AllowedSortFields = ["Slug"];
        public static readonly string[] AllowedFilterFields = ["Slug"];
    }
}
