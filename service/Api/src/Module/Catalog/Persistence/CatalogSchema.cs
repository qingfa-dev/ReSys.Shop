namespace Module.Catalog.Persistence;

/// <summary>
/// Contains shared constant values for the Identity module.
/// </summary>
public static class CatalogSchema
{
    /// <summary>
    /// The database schema name used by the Identity module.
    /// </summary>
    public const string Name = "catalog";

    /// <summary>
    /// Constant values for database table names.
    /// </summary>
    public static class TableNames
    {
        public const string Products = "products";
        public const string Variants = "variants";
        public const string VariantImages = "variant_images";
        public const string VariantImageEmbeddings = "variant_image_embeddings";
        public const string OptionTypes = "option_types";
        public const string OptionValues = "option_values";
        public const string Taxonomies = "taxonomies";
        public const string Taxa = "taxa";
        public const string TaxonRules = "taxon_rules";
        public const string Classifications = "classifications";
        public const string OptionValueVariants = "option_value_variants";
        public const string Prices = "prices";
    }
}