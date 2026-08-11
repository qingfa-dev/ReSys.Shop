namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Storefront
    {
        public static class Products
        {
            public static class Get
            {
                public static class ById
                {
                    public const string Route = "api/storefront/catalog/products/{id}";
                    public const string Description = "Retrieve full product detail page for the storefront by ID";
                    public const string Summary = "Get product detail page";
                }

                public static class Related
                {
                    public const string Route = "api/storefront/catalog/products/related";
                    public const string Description = "Retrieve related products for a given product (productId query)";
                    public const string Summary = "Get related products";
                }

                public static class Similar
                {
                    public const string Route = "api/storefront/catalog/products/similar";
                    public const string Description = "Retrieve visually similar products using image embedding similarity (productId query)";
                    public const string Summary = "Get similar products by image";
                }

                public static class PagedOrAll
                {
                    public const string Route = "api/storefront/catalog/products";
                    public const string Description = "Unified product listing with optional text search, faceted filters, sorting, and pagination";
                    public const string Summary = "List or search products";
                }

            }

            public static class Images
            {
                public static class Get
                {
                    public const string Route = "api/storefront/catalog/products/images/{id}";
                    public const string Description = "Display a variant image file inline by its ID";
                    public const string Summary = "Display image";
                }
                public static class Search
                {
                    public const string Route = "api/storefront/catalog/products/images/search";
                    public const string Description = "Search products by uploading an image for visual similarity";
                    public const string Summary = "Search by image upload";
                }
            }
        }

        public static class Taxonomies
        {
            public static class Get
            {
                public const string Route = "api/storefront/catalog/taxonomies";
                public const string Description = "Retrieve classification tree with nested taxons for mega-menu";
                public const string Summary = "Get classification tree";
            }

            public static class Taxons
            {
                public static class Get
                {
                    public const string Route = "api/storefront/catalog/taxonomies/taxons";
                    public const string Description = "Retrieve taxons filtered by depth and taxonomy";
                    public const string Summary = "List taxons";
                }
            }
        }

        public static class OptionTypes
        {
            public const string Route = "api/storefront/catalog/option-types";
            public const string Description = "Retrieve all option types with values for filter facets";
            public const string Summary = "List option types";

            public static class Values
            {
                public const string Route = "api/storefront/catalog/option-types/values";
                public const string Description = "Retrieve all option values for filter facets";
                public const string Summary = "List option values";
            }
        }
    }
}
