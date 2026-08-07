namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Products
        {
            private const string BaseRoute = $"{Route}/products";

            public static class Detail
            {
                public const string Route = $"{BaseRoute}/{{slug}}";
                public const string Description = "Retrieve full product detail page for the storefront by slug";
                public const string Summary = "Get product detail page";
            }

            public static class Related
            {
                public const string Route = $"{BaseRoute}/related";
                public const string Description = "Retrieve related products for a given product (productId query)";
                public const string Summary = "Get related products";
            }

            public static class Similar
            {
                public const string Route = $"{BaseRoute}/similar";
                public const string Description = "Retrieve visually similar products using image embedding similarity (productId query)";
                public const string Summary = "Get similar products by image";
            }



            public static class List
            {
                public const string Route = BaseRoute;
                public const string Description = "Unified product listing with optional text search, faceted filters, sorting, and pagination";
                public const string Summary = "List or search products";
            }

           

            public static class Images
            {
                private const string BaseImagesRoute = $"{BaseRoute}/images";

                public static class Get
                {
                    public const string Route = $"{BaseImagesRoute}/{{id}}";
                    public const string Description = "Display a variant image file inline by its ID";
                    public const string Summary = "Display image";
                }
                public static class Search
                {
                    public const string Route = $"{BaseImagesRoute}/search";
                    public const string Description = "Search products by uploading an image for visual similarity";
                    public const string Summary = "Search by image upload";
                }
            }

            public static class VisualSearchModels
            {
                public const string Route = $"{BaseRoute}/visual-search/models";
                public const string Description = "List available visual search embedding models";
                public const string Summary = "List visual search models";
            }
        }

        public static class Classifications
        {
            private const string BaseRoute = Storefront.Route;
            public static class Taxonomies
            {
                public const string Route = $"{BaseRoute}/taxonomies";
                public const string Description = "Retrieve classification tree with nested taxons for mega-menu";
                public const string Summary = "Get classification tree";
            }

            public static class Taxons
            {
                public const string Route = $"{BaseRoute}/taxons";

                public const string Description = "Retrieve taxons filtered by depth and taxonomy";
                public const string Summary = "List taxons";

                public static class All
                {
                    public const string Route = $"{BaseRoute}/taxons/all";
                    public const string Description = "Retrieve all taxons without filtering";
                    public const string Summary = "List all taxons";
                }
            }
        }

        public static class Options
        {
            private const string BaseRoute = Storefront.Route;

            public static class OptionTypes
            {
                public const string Route = $"{BaseRoute}/option-types";
                public const string Description = "Retrieve all option types with values for filter facets";
                public const string Summary = "List option types";
            }
            public static class OptionValues
            {
                public const string Route = $"{BaseRoute}/option-values";
                public const string Description = "Retrieve all option values for filter facets";
                public const string Summary = "List option values";
            }
        }
    }
}