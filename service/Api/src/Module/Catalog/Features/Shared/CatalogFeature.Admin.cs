using Shared.Security.Identity.Domain.Permissions;

namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Admin
    {
        public const string Route = "api/catalog";

        public static class OptionTypes
        {
            private const string BaseRoute = $"{Route}/option-types";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new option type";
                public const string Summary = "Create option type";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionTypes.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all option types";
                public const string Summary = "Get all option types";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionTypes.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an option type by identifier";
                public const string Summary = "Get option type by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionTypes.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing option type";
                public const string Summary = "Update option type";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionTypes.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete an option type";
                public const string Summary = "Delete option type";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionTypes.Delete;
            }
        }

        public static class OptionValues
        {
            private const string BaseRoute = $"{Route}/option-values";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new option value";
                public const string Summary = "Create option value";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionValues.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all option values";
                public const string Summary = "Get all option values";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionValues.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an option value by identifier";
                public const string Summary = "Get option value by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionValues.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing option value";
                public const string Summary = "Update option value";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionValues.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete an option value";
                public const string Summary = "Delete option value";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.OptionValues.Delete;
            }
        }

        public static class Taxonomies
        {
            private const string BaseRoute = $"{Route}/taxonomies";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new taxonomy";
                public const string Summary = "Create taxonomy";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all taxonomies";
                public const string Summary = "Get all taxonomies";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a taxonomy by identifier";
                public const string Summary = "Get taxonomy by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing taxonomy";
                public const string Summary = "Update taxonomy";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a taxonomy";
                public const string Summary = "Delete taxonomy";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.Delete;
            }

            public static class Restore
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/restore";
                public const string Description = "Restore a taxonomy";
                public const string Summary = "Restore taxonomy";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxonomies.Restore;
            }
        }

        public static class Taxons
        {
            private const string BaseRoute = $"{Route}/taxons";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new taxon";
                public const string Summary = "Create taxon";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all taxons";
                public const string Summary = "Get all taxons";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a taxon by identifier";
                public const string Summary = "Get taxon by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.List;
            }

            public static class GetTree
            {
                public const string Route = $"{BaseRoute}/tree";
                public const string Description = "Retrieve the taxon tree for a taxonomy (taxonomyId query)";
                public const string Summary = "Get taxon tree";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.List;
            }

            public static class GetList
            {
                public const string Route = $"{BaseRoute}/list";
                public const string Description = "Retrieve paged taxon list for a taxonomy (taxonomyId query)";
                public const string Summary = "Get taxon list";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing taxon";
                public const string Summary = "Update taxon";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a taxon";
                public const string Summary = "Delete taxon";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.Delete;
            }

            public static class Restore
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/restore";
                public const string Description = "Restore a taxon";
                public const string Summary = "Restore taxon";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.Restore;
            }

            public static class Reposition
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/reposition";
                public const string Description = "Reposition a taxon";
                public const string Summary = "Reposition taxon";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.Update;
            }
        }

        public static class TaxonRules
        {
            private const string BaseRoute = $"{Route}/taxon-rules";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new rule for a taxon (TaxonId in body)";
                public const string Summary = "Create taxon rule";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.ManageRules;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all rules for a taxon (taxonId query)";
                public const string Summary = "Get taxon rules";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{ruleId:guid}}";
                public const string Description = "Update an existing rule for a taxon (TaxonId in body)";
                public const string Summary = "Update taxon rule";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.ManageRules;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{ruleId:guid}}";
                public const string Description = "Delete a rule from a taxon (TaxonId in body)";
                public const string Summary = "Delete taxon rule";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.ManageRules;
            }

            public static class Sync
            {
                public const string Route = $"{BaseRoute}/sync";
                public const string Description = "Synchronize full rule list for a taxon (TaxonId in body)";
                public const string Summary = "Sync taxon rules";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Taxons.ManageRules;
            }
        }

        public static class Products
        {
            private const string BaseRoute = $"{Route}/products";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new product";
                public const string Summary = "Create product";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all products";
                public const string Summary = "Get all products";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a product by identifier";
                public const string Summary = "Get product by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing product";
                public const string Summary = "Update product";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a product";
                public const string Summary = "Delete product";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.Delete;
            }

            public static class Activate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/activate";
                public const string Description = "Activate a product";
                public const string Summary = "Activate product";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.Manage;
            }

            public static class Discontinue
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/discontinue";
                public const string Description = "Discontinue a product";
                public const string Summary = "Discontinue product";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Products.Manage;
            }
        }

        public static class ProductOptionTypes
        {
            private const string BaseRoute = $"{Route}/product-option-types";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve option types assigned to a product (productId query)";
                public const string Summary = "Get product option types";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsOptionTypes.Read;
            }

            public static class Assign
            {
                public const string Route = $"{BaseRoute}/assign";
                public const string Description = "Assign option types to a product (ProductId in body)";
                public const string Summary = "Assign product option types";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsOptionTypes.Assign;
            }

            public static class Revoke
            {
                public const string Route = $"{BaseRoute}/revoke";
                public const string Description = "Revoke option types from a product (ProductId in body)";
                public const string Summary = "Revoke product option types";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsOptionTypes.Revoke;
            }

            public static class Sync
            {
                public const string Route = $"{BaseRoute}/sync";
                public const string Description = "Synchronize option types for a product (ProductId in body)";
                public const string Summary = "Sync product option types";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsOptionTypes.Sync;
            }
        }

        public static class ProductClassifications
        {
            private const string BaseRoute = $"{Route}/product-classifications";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve classifications (taxon assignments) for a product (productId query)";
                public const string Summary = "Get product classifications";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsClassifications.Read;
            }

            public static class Assign
            {
                public const string Route = $"{BaseRoute}/assign";
                public const string Description = "Assign taxons to a product (ProductId in body)";
                public const string Summary = "Assign product classifications";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsClassifications.Assign;
            }

            public static class Revoke
            {
                public const string Route = $"{BaseRoute}/revoke";
                public const string Description = "Revoke taxon classifications from a product (ProductId in body)";
                public const string Summary = "Revoke product classifications";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsClassifications.Revoke;
            }

            public static class Sync
            {
                public const string Route = $"{BaseRoute}/sync";
                public const string Description = "Synchronize taxon classifications for a product (ProductId in body)";
                public const string Summary = "Sync product classifications";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.ProductsClassifications.Sync;
            }
        }

        public static class Variants
        {
            private const string BaseRoute = $"{Route}/variants";

            public static class Add
            {
                public const string Route = BaseRoute;
                public const string Description = "Add a new variant to a product";
                public const string Summary = "Add variant";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all variants";
                public const string Summary = "Get all variants";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a variant by identifier";
                public const string Summary = "Get variant by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing variant";
                public const string Summary = "Update variant";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a variant";
                public const string Summary = "Delete variant";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.Delete;
            }
        }

        public static class VariantPrices
        {
            private const string BaseRoute = $"{Route}/variant-prices";

            public static class Set
            {
                public const string Route = BaseRoute;
                public const string Description = "Set price for a variant (upsert by Currency/CountryIso; VariantId in body)";
                public const string Summary = "Set variant price";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.ManagePrice;
            }

            public static class List
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve prices for a variant (paged or all; variantId query)";
                public const string Summary = "List variant prices";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.List;
            }

            public static class Remove
            {
                public const string Route = $"{BaseRoute}/{{priceId:guid}}";
                public const string Description = "Delete a price for a variant (VariantId in body)";
                public const string Summary = "Delete variant price";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.ManagePrice;
            }

            public static class Sync
            {
                public const string Route = $"{BaseRoute}/sync";
                public const string Description = "Synchronize full price list for a variant (VariantId in body)";
                public const string Summary = "Sync variant prices";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.Variants.ManagePrice;
            }
        }

        public static class VariantOptionValues
        {
            private const string BaseRoute = $"{Route}/variant-option-values";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all option values with assignment status for a variant (variantId query)";
                public const string Summary = "Get variant option values";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantOptionValues.List;
            }

            public static class Assign
            {
                public const string Route = $"{BaseRoute}/assign";
                public const string Description = "Assign option values to a variant (VariantId in body)";
                public const string Summary = "Assign variant option values";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantOptionValues.Manage;
            }

            public static class Revoke
            {
                public const string Route = $"{BaseRoute}/revoke";
                public const string Description = "Revoke option values from a variant (VariantId in body)";
                public const string Summary = "Revoke variant option values";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantOptionValues.Manage;
            }

            public static class Sync
            {
                public const string Route = $"{BaseRoute}/sync";
                public const string Description = "Synchronize full set of option values for a variant (VariantId in body)";
                public const string Summary = "Sync variant option values";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantOptionValues.Manage;
            }
        }

        public static class VariantImages
        {
            private const string BaseRoute = $"{Route}/variant-images";

            public static class Upload
            {
                public const string Route = BaseRoute;
                public const string Description = "Upload a new image for a variant (VariantId in form)";
                public const string Summary = "Upload variant image";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.Upload;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all images for a variant (variantId query)";
                public const string Summary = "List variant images";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a variant image by identifier";
                public const string Summary = "Get variant image by ID";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update variant image details (alt, position, type)";
                public const string Summary = "Update variant image";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a variant image";
                public const string Summary = "Delete variant image";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.Delete;
            }

            public static class Download
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/download";
                public const string Description = "Download the variant image file";
                public const string Summary = "Download variant image";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.List;
            }
        }

        public static class VariantImageEmbeddings
        {
            private const string BaseRoute = $"{Route}/variant-image-embeddings";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Generate embedding for a variant image (VariantImageId in body)";
                public const string Summary = "Create image embedding";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
            }

            public static class Regenerate
            {
                public const string Route = $"{BaseRoute}/regenerate";
                public const string Description = "Regenerate embedding for a variant image with a new model version (VariantImageId in body)";
                public const string Summary = "Regenerate image embedding";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
            }

            public static class Get
            {
                public const string Route = $"{BaseRoute}/{{variantImageId:guid}}";
                public const string Description = "Get the embedding for a variant image";
                public const string Summary = "Get image embedding";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{variantImageId:guid}}";
                public const string Description = "Delete the embedding for a variant image";
                public const string Summary = "Delete image embedding";
                public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
            }
        }
    }
}
