namespace Module.Catalog.Domain.OptionTypes.Values;

/// <summary>
/// Provides a centralized location for option value-related constants, including API routes and tags.
/// </summary>
public static class OptionValueFeature
{
    /// <summary>
    /// Defines the API routes for option value features.
    /// </summary>
    public static class Routes
    {
        public const string Base = "api/catalog/option-types/{OptionTypeId:guid}/values";

        public const string GetPagedEndpoint = Base;
        public const string SyncEndpoint = Base;

        public const string CreateEndpoint = Base;
        public const string UpdateEndpoint = $"{Base}/{{Id:guid}}";
        public const string DeleteEndpoint = $"{Base}/{{Id:guid}}";
        public const string GetByIdEndpoint = $"{Base}/{{Id:guid}}";
    }

    /// <summary>
    /// Defines the tags used for grouping option value endpoints in documentation.
    /// </summary>
    public static class Tags
    {
        public const string OptionValue = "OptionValues";
    }
}
