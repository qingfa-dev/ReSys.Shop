namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Products
        {
            public const string BaseRoute = $"{Storefront.Route}/products";

            public static class Get
            {
                public static class Detail
                {
                    public const string Route = $"{BaseRoute}/{{slug}}";
                    public const string Description = "Retrieve full product detail page for the storefront by slug";
                    public const string Summary = "Get product detail page";
                }

                public static class Availability
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/availability";
                    public const string Description = "Retrieve style matrix availability grid for a product";
                    public const string Summary = "Get product availability";
                }

                public static class Related
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/related";
                    public const string Description = "Retrieve related products for a given product";
                    public const string Summary = "Get related products";
                }

                public static class Similar
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/similar";
                    public const string Description = "Retrieve visually similar products using image embedding similarity";
                    public const string Summary = "Get similar products by image";
                }

                public static class SearchByImage
                {
                    public const string Route = $"{Storefront.Route}/search-by-image";
                    public const string Description = "Search products by uploading an image for visual similarity";
                    public const string Summary = "Search by image upload";
                }

                public static class List
                {
                    public const string Route = BaseRoute;
                    public const string Description = "Unified product listing with optional text search, faceted filters, sorting, and pagination";
                    public const string Summary = "List or search products";
                }
            }
        }

        public static class Taxonomies
        {
            public const string BaseRoute = $"{Storefront.Route}/taxonomies";

            public static class Get
            {
                public static class Tree
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}";
                    public const string Description = "Retrieve taxonomy tree with nested taxons for mega-menu";
                    public const string Summary = "Get taxonomy tree";
                }
            }
        }

        public static class Taxons
        {
            public const string BaseRoute = $"{Storefront.Route}/taxons";

            public static class Get
            {
                public static class All
                {
                    public const string Route = BaseRoute;
                    public const string Description = "Retrieve taxons filtered by depth and taxonomy";
                    public const string Summary = "List taxons";
                }

                public static class Products
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/products";
                    public const string Description = "Retrieve paginated products by taxon with sorting";
                    public const string Summary = "Get products by taxon";
                }
            }
        }

        public static class OptionTypes
        {
            public static class Get
            {
                public static class All
                {
                    public const string Route = $"{Storefront.Route}/option-types";
                    public const string Description = "Retrieve all option types with values for filter facets";
                    public const string Summary = "List option types";
                }
            }
        }

        public static class Images
        {
            public static class Get
            {
                public static class Image
                {
                    public const string Route = $"{Storefront.Route}/images/{{id:guid}}";
                    public const string Description = "Display a variant image file inline by its ID";
                    public const string Summary = "Display image";
                }
            }
        }
    }
}
