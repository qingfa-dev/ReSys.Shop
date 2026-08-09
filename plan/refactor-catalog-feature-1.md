---
goal: Flatten the nested CatalogFeature endpoint-constant classes into sibling entity groups and relocate nested parent ids out of URL paths into request bodies (commands) and query parameters (queries) for the Admin and Storefront API surfaces.
version: 1.0
date_created: 2026-08-01
last_updated: 2026-08-01
owner: ReSys.Shop Engineering
status: 'Planned'
tags: [`refactor`, `catalog`, `api`, `routing`, `csharp`]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The current `CatalogFeature` (partial class in `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` and `CatalogFeature.Storefront.cs`) nests entity groups up to four levels deep (e.g. `Admin.Products.Variants.Prices.Set`), and the corresponding URLs bury parent entity ids inside the path (e.g. `api/admin/catalog/products/variants/{variantId}/prices`). This plan rewrites the two `CatalogFeature` partial classes into a flat shape (sibling entity groups, each with action classes exposing `Route`, `Description`, `Summary`, `Permission`), flattens the URL routes, and relocates nested parent ids to the request body for commands and to query-string parameters for queries, following the human-confirmed design rules. Feature directories and namespaces are intentionally left untouched; only constant paths, endpoint wiring, request records, and tests change.

## 1. Requirements & Constraints

- **REQ-001**: Rewrite `CatalogFeature.Admin` so every entity group is a sibling class under `Admin` (OptionTypes, OptionValues, Taxonomies, Taxons, TaxonRules, Products, ProductOptionTypes, ProductClassifications, Variants, VariantPrices, VariantOptionValues, VariantImages, VariantImageEmbeddings), each with a private `BaseRoute` const and action classes exposing `Route`, `Description`, `Summary`, and `Permission` exactly as in the target code in section 2 (Reference).
- **REQ-002**: Rewrite `CatalogFeature.Storefront` so every entity group is a sibling class under `Storefront` (Products, Taxonomies, Taxons, OptionTypes, Images) with the `Get` verb layer removed (e.g. `Products.Get.Detail` becomes `Products.Detail`).
- **REQ-003**: Flatten URL routes: remove nested path segments (e.g. `api/admin/catalog/option-types/option-values` -> `api/admin/catalog/option-values`; `api/admin/catalog/products/variants/{id}` -> `api/admin/catalog/variants/{id}`) exactly as listed in the Route Mapping table in section 2 (Reference).
- **REQ-004**: Relocate nested parent ids that were URL path segments: for commands (POST/PUT/DELETE) move the parent id into the request body; for queries (GET) move the parent id into a query-string parameter.
- **REQ-005**: Do not move feature directories or change C# namespaces of `Features/Admin/**` or `Features/Storefront/**` files. Only `Endpoint.cs` wiring, action `Request` records, and `CatalogFeature.*` constant references change.
- **SEC-001**: Preserve every `HasPermission(...)` binding. Permissions continue to reference the unchanged `CatalogFeatureMetadata` members; only the C# constant path to reach them changes.
- **CON-001**: `TreatWarningsAsErrors=true` globally; the build must complete with `0 Warning(s), 0 Error(s)`.
- **CON-002**: `CatalogFeatureMetadata` (`service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs`) is the source of truth for permissions and must NOT be modified by this plan.
- **CON-003**: `CatalogFeature.Tags` (`CatalogFeature.Tags.cs`) must NOT be modified.
- **CON-004**: `CatalogFeature.Admin.Route = "api/admin/catalog"` and `CatalogFeature.Storefront.Route = "api/storefront"` remain unchanged.
- **GUD-001** (path-only pattern): For an endpoint whose group moved but whose action carries no parent id, update only the `CatalogFeature.Admin.*` / `CatalogFeature.Storefront.*` constant references to the new flat path; leave the endpoint lambda, command/query construction, and handler untouched. Example: `CatalogFeature.Admin.OptionTypes.OptionValues.GetById.Route` becomes `CatalogFeature.Admin.OptionValues.GetById.Route`.
- **GUD-002** (query parent-id pattern): For a GET endpoint whose parent id leaves the path, remove the route segment, change the lambda parameter to `[FromQuery] Guid <parentId>` (e.g. `productId`, `taxonId`, `variantId`), and pass it positionally to the existing `Query(...)` constructor. The `Query` record and handler are NOT modified.
- **GUD-003** (command parent-id pattern): For a POST/PUT command with an existing body `Request`, add a `Guid <ParentId>` init property to that action's `Request` record, drop the route parent-id lambda parameter, and change construction to `new Command(request.<ParentId>, request)`. The `Command` record and handler are NOT modified.
- **GUD-004** (delete-without-body pattern): For a DELETE command that currently has no body (`RemoveVariantPrice`, `DeleteTaxonRule`), create an action `Request` record carrying both ids, change construction to `new Command(request.<ParentId>, <childRouteId>)`, and bind `[FromBody] Request request` plus `[FromRoute] Guid <childRouteId>` in the endpoint. The `Command` record and handler are NOT modified.
- **GUD-005** (multipart upload pattern): For `UploadVariantImage`, add `Guid VariantId` to the shared `UploadImageRequest` model (inherited by the action `Request`), drop the route param, and construct `new Command(request.VariantId, request)`.
- **GUD-006**: Preserve all existing `// Contract:`, `// Load:`, `// Map:`, `// Route:` comment lines verbatim in every modified file. Update only the route text inside `/// Route:` doc comments to the new URL.
- **PAT-001**: Flattened class shape — `public static partial class CatalogFeature { public static class Admin { public const string Route = "api/admin/catalog"; public static class <Entity> { private const string BaseRoute = $"{Route}/<entity>"; public static class <Action> { public const string Route = BaseRoute; /* or $"{BaseRoute}/{{id:guid}}" */ public const string Description = "..."; public const string Summary = "..."; public static PermissionMetadata Permission => CatalogFeatureMetadata.<Group>.<Action>; } } } }`.

## 2. Implementation Steps

### Reference: Target Flattened Structure

The following two files replace the current `CatalogFeature.Admin.cs` and `CatalogFeature.Storefront.cs`. Apply these verbatim (whitespace/formatting may be normalized by the formatter but semantics must match).

`service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` (target):

```csharp
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Admin
    {
        public const string Route = "api/admin/catalog";

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
        }
    }
}
```

`service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` (target):

```csharp
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

            public static class Availability
            {
                public const string Route = $"{BaseRoute}/availability";
                public const string Description = "Retrieve style matrix availability grid for a product (productId query)";
                public const string Summary = "Get product availability";
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

            public static class SearchByImage
            {
                public const string Route = $"{Route}/search-by-image";
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

        public static class Taxonomies
        {
            private const string BaseRoute = $"{Route}/taxonomies";

            public static class Tree
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve taxonomy tree with nested taxons for mega-menu";
                public const string Summary = "Get taxonomy tree";
            }
        }

        public static class Taxons
        {
            private const string BaseRoute = $"{Route}/taxons";

            public static class All
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve taxons filtered by depth and taxonomy";
                public const string Summary = "List taxons";
            }

            public static class Products
            {
                public const string Route = $"{BaseRoute}/products";
                public const string Description = "Retrieve paginated products by taxon (taxonId query)";
                public const string Summary = "Get products by taxon";
            }
        }

        public static class OptionTypes
        {
            private const string BaseRoute = $"{Route}/option-types";

            public static class All
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all option types with values for filter facets";
                public const string Summary = "List option types";
            }
        }

        public static class Images
        {
            private const string BaseRoute = $"{Route}/images";

            public static class Image
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Display a variant image file inline by its ID";
                public const string Summary = "Display image";
            }
        }
    }
}
```

### Reference: Route Mapping

| Old constant path (current) | New constant path (target) | Old URL | New URL |
|------|------|------|------|
| `Admin.OptionTypes.*` | unchanged | `api/admin/catalog/option-types...` | unchanged |
| `Admin.OptionTypes.OptionValues.Create/GetAll/GetById/Update/Delete.Route` | `Admin.OptionValues.Create/GetAll/GetById/Update/Delete.Route` | `api/admin/catalog/option-types/option-values[/{id:guid}]` | `api/admin/catalog/option-values[/{id:guid}]` |
| `Admin.Taxonomies.*` | unchanged | `api/admin/catalog/taxonomies...` | unchanged |
| `Admin.Taxonomies.Taxons.Create/GetAll/GetById/Update/Delete.Route` | `Admin.Taxons.Create/GetAll/GetById/Update/Delete.Route` | `api/admin/catalog/taxonomies/taxons[/{id:guid}]` | `api/admin/catalog/taxons[/{id:guid}]` |
| `Admin.Taxonomies.Taxons.GetTree.Route` | `Admin.Taxons.GetTree.Route` | `api/admin/catalog/taxonomies/taxons/tree` | `api/admin/catalog/taxons/tree` |
| `Admin.Taxonomies.Taxons.GetList.Route` | `Admin.Taxons.GetList.Route` | `api/admin/catalog/taxonomies/taxons/list` | `api/admin/catalog/taxons/list` |
| `Admin.Taxonomies.Taxons.Restore.Route` | `Admin.Taxons.Restore.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/restore` | `api/admin/catalog/taxons/{id:guid}/restore` |
| `Admin.Taxonomies.Taxons.Reposition.Route` | `Admin.Taxons.Reposition.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/reposition` | `api/admin/catalog/taxons/{id:guid}/reposition` |
| `Admin.Taxonomies.Taxons.Rules.Create.Route` | `Admin.TaxonRules.Create.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/rules` | `api/admin/catalog/taxon-rules` (body `TaxonId`) |
| `Admin.Taxonomies.Taxons.Rules.GetAll.Route` | `Admin.TaxonRules.GetAll.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/rules` | `api/admin/catalog/taxon-rules` (`?taxonId=`) |
| `Admin.Taxonomies.Taxons.Rules.Update.Route` | `Admin.TaxonRules.Update.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/rules/{ruleId:guid}` | `api/admin/catalog/taxon-rules/{ruleId:guid}` (body `TaxonId`) |
| `Admin.Taxonomies.Taxons.Rules.Delete.Route` | `Admin.TaxonRules.Delete.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/rules/{ruleId:guid}` | `api/admin/catalog/taxon-rules/{ruleId:guid}` (body `TaxonId`) |
| `Admin.Taxonomies.Taxons.Rules.Sync.Route` | `Admin.TaxonRules.Sync.Route` | `api/admin/catalog/taxonomies/taxons/{id:guid}/rules/sync` | `api/admin/catalog/taxon-rules/sync` (body `TaxonId`) |
| `Admin.Products.*` | unchanged | `api/admin/catalog/products...` | unchanged |
| `Admin.Products.OptionTypes.Get.Route` | `Admin.ProductOptionTypes.Get.Route` | `api/admin/catalog/products/{id:guid}/option-types` | `api/admin/catalog/product-option-types` (`?productId=`) |
| `Admin.Products.OptionTypes.Assign/Revoke/Sync.Route` | `Admin.ProductOptionTypes.Assign/Revoke/Sync.Route` | `api/admin/catalog/products/{id:guid}/option-types/{verb}` | `api/admin/catalog/product-option-types/{verb}` (body `ProductId`) |
| `Admin.Products.Classifications.Get.Route` | `Admin.ProductClassifications.Get.Route` | `api/admin/catalog/products/{id:guid}/classifications` | `api/admin/catalog/product-classifications` (`?productId=`) |
| `Admin.Products.Classifications.Assign/Revoke/Sync.Route` | `Admin.ProductClassifications.Assign/Revoke/Sync.Route` | `api/admin/catalog/products/{id:guid}/classifications/{verb}` | `api/admin/catalog/product-classifications/{verb}` (body `ProductId`) |
| `Admin.Products.Variants.Add.Route` | `Admin.Variants.Add.Route` | `api/admin/catalog/products/variants` | `api/admin/catalog/variants` |
| `Admin.Products.Variants.GetAll.Route` | `Admin.Variants.GetAll.Route` | `api/admin/catalog/products/variants` | `api/admin/catalog/variants` |
| `Admin.Products.Variants.GetById/Update/Delete.Route` | `Admin.Variants.GetById/Update/Delete.Route` | `api/admin/catalog/products/variants/{id:guid}` | `api/admin/catalog/variants/{id:guid}` |
| `Admin.Products.Variants.Prices.Set.Route` | `Admin.VariantPrices.Set.Route` | `api/admin/catalog/products/variants/{variantId:guid}/prices` | `api/admin/catalog/variant-prices` (body `VariantId`) |
| `Admin.Products.Variants.Prices.List.Route` | `Admin.VariantPrices.List.Route` | `api/admin/catalog/products/variants/{variantId:guid}/prices` | `api/admin/catalog/variant-prices` (`?variantId=`) |
| `Admin.Products.Variants.Prices.Remove.Route` | `Admin.VariantPrices.Remove.Route` | `api/admin/catalog/products/variants/{variantId:guid}/prices/{priceId:guid}` | `api/admin/catalog/variant-prices/{priceId:guid}` (body `VariantId`) |
| `Admin.Products.Variants.Prices.Sync.Route` | `Admin.VariantPrices.Sync.Route` | `api/admin/catalog/products/variants/{variantId:guid}/prices/sync` | `api/admin/catalog/variant-prices/sync` (body `VariantId`) |
| `Admin.Products.Variants.OptionValues.Get.Route` | `Admin.VariantOptionValues.Get.Route` | `api/admin/catalog/products/variants/{variantId:guid}/option-values` | `api/admin/catalog/variant-option-values` (`?variantId=`) |
| `Admin.Products.Variants.OptionValues.Assign/Revoke/Sync.Route` | `Admin.VariantOptionValues.Assign/Revoke/Sync.Route` | `api/admin/catalog/products/variants/{variantId:guid}/option-values/{verb}` | `api/admin/catalog/variant-option-values/{verb}` (body `VariantId`) |
| `Admin.Products.Variants.Images.Upload.Route` | `Admin.VariantImages.Upload.Route` | `api/admin/catalog/products/variants/{variantId:guid}/images` | `api/admin/catalog/variant-images` (form `VariantId`) |
| `Admin.Products.Variants.Images.GetAll.Route` | `Admin.VariantImages.GetAll.Route` | `api/admin/catalog/products/variants/{variantId:guid}/images` | `api/admin/catalog/variant-images` (`?variantId=`) |
| `Admin.Products.Variants.Images.GetById/Update/Delete.Route` | `Admin.VariantImages.GetById/Update/Delete.Route` | `api/admin/catalog/products/variants/images/{id:guid}` | `api/admin/catalog/variant-images/{id:guid}` |
| `Admin.Products.Variants.Images.Download.Route` | `Admin.VariantImages.Download.Route` | `api/admin/catalog/products/variants/images/{id:guid}/download` | `api/admin/catalog/variant-images/{id:guid}/download` |
| `Admin.Products.Variants.Images.Embeddings.Create.Route` | `Admin.VariantImageEmbeddings.Create.Route` | `api/admin/catalog/products/variants/images/{id:guid}/embeddings` | `api/admin/catalog/variant-image-embeddings` (body `VariantImageId`) |
| `Admin.Products.Variants.Images.Embeddings.Regenerate.Route` | `Admin.VariantImageEmbeddings.Regenerate.Route` | `api/admin/catalog/products/variants/images/{id:guid}/embeddings` | `api/admin/catalog/variant-image-embeddings/regenerate` (body `VariantImageId`) |
| `Storefront.Products.Get.Detail.Route` | `Storefront.Products.Detail.Route` | `api/storefront/products/{slug}` | unchanged |
| `Storefront.Products.Get.Availability.Route` | `Storefront.Products.Availability.Route` | `api/storefront/products/{id:guid}/availability` | `api/storefront/products/availability` (`?productId=`) |
| `Storefront.Products.Get.Related.Route` | `Storefront.Products.Related.Route` | `api/storefront/products/{id:guid}/related` | `api/storefront/products/related` (`?productId=`) |
| `Storefront.Products.Get.Similar.Route` | `Storefront.Products.Similar.Route` | `api/storefront/products/{id:guid}/similar` | `api/storefront/products/similar` (`?productId=`) |
| `Storefront.Products.SearchByImage.Route` | `Storefront.Products.SearchByImage.Route` | `api/storefront/search-by-image` | unchanged |
| `Storefront.Products.Get.List.Route` | `Storefront.Products.List.Route` | `api/storefront/products` | unchanged |
| `Storefront.Taxonomies.Get.Tree.Route` | `Storefront.Taxonomies.Tree.Route` | `api/storefront/taxonomies/{id:guid}` | unchanged |
| `Storefront.Taxons.Get.All.Route` | `Storefront.Taxons.All.Route` | `api/storefront/taxons` | unchanged |
| `Storefront.Taxons.Get.Products.Route` | `Storefront.Taxons.Products.Route` | `api/storefront/taxons/{id:guid}/products` | `api/storefront/taxons/products` (`?taxonId=`) |
| `Storefront.OptionTypes.Get.All.Route` | `Storefront.OptionTypes.All.Route` | `api/storefront/option-types` | unchanged |
| `Storefront.Images.Get.Image.Route` | `Storefront.Images.Image.Route` | `api/storefront/images/{id:guid}` | unchanged |

### Reference: Canonical Transformation Examples

Pattern Q (query parent-id -> query param). Before: `MapGet(Admin.Products.OptionTypes.Get.Route, (Guid id, [AsParameters] Parameters p, ...) => new Query(id, p))`. After: `MapGet(Admin.ProductOptionTypes.Get.Route, ([FromQuery] Guid productId, [AsParameters] Parameters p, ...) => new Query(productId, p))`. Route loses the `{id}` segment; `Query` record and handler unchanged.

Pattern C (command parent-id -> body). Before: `MapPost(..., (Guid id, [FromBody] Request r, ...) => new Command(id, r))`. After: `MapPost(..., ([FromBody] Request r, ...) => new Command(r.ProductId, r))`. The action `Request` record gains `public Guid ProductId { get; init; }`; `Command` record and handler unchanged.

Pattern D (delete without body). Before: `MapDelete(..., (Guid id, Guid ruleId, ...) => new Command(id, ruleId))`. After: `MapDelete(..., ([FromRoute] Guid ruleId, [FromBody] Request r, ...) => new Command(r.TaxonId, ruleId))`. New action `Request` record: `public sealed record Request { public Guid TaxonId { get; init; } public Guid RuleId { get; init; } }`; `Command` record and handler unchanged.

Pattern U (multipart upload). Before: `MapPost(..., ([FromRoute] Guid variantId, [FromForm] Request r, ...) => new Command(variantId, r))`. After: `MapPost(..., ([FromForm] Request r, ...) => new Command(r.VariantId, r))`. The shared `UploadImageRequest` model gains `public Guid VariantId { get; init; }`; `Command` record and handler unchanged.

Pattern E (embedding). Before: `MapPost(..., ([FromRoute] Guid id, [FromBody] Request? r, ...) => new Command(new Request { VariantImageId = id, ... }))`. After: `MapPost(..., ([FromBody] Request r, ...) => new Command(new Request { VariantImageId = r.VariantImageId, ... }))`. The endpoint no longer supplies `id`; `Command` and handler unchanged.

### Implementation Phase 1

- GOAL-001: Rewrite `CatalogFeature.Admin` into the flat sibling-entity-group structure and provide the canonical route mapping used by every later phase.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Overwrite `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` with the target `Admin` code in "Reference: Target Flattened Structure". Verify it still contains `public static partial class CatalogFeature` and the `using Shared.Security.Identity.Domain.Permissions;` import. Do NOT modify `CatalogFeatureMetadata.cs` or `CatalogFeature.Tags.cs`. | |  |

### Implementation Phase 2

- GOAL-002: Update OptionValues and Taxons endpoint files that only reference moved constant paths (no parent-id relocation).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | In the 5 OptionValues endpoint files under `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/OptionValues/` (`Create/CreateOptionValue.Endpoint.cs`, `Delete/DeleteOptionValue.Endpoint.cs`, `Update/UpdateOptionValue.Endpoint.cs`, `Get/Paged/GetOptionValuesPaged.Endpoint.cs`, `Get/ById/GetOptionValueById.Endpoint.cs`) replace the constant references `CatalogFeature.Admin.OptionTypes.OptionValues.X` with `CatalogFeature.Admin.OptionValues.X` (X = Create, Delete, Update, GetAll, GetById). No route/verb/param changes. | |  |
| TASK-003 | In the 9 Taxon endpoint files under `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/` (`Create/CreateTaxon.Endpoint.cs`, `Delete/DeleteTaxon.Endpoint.cs`, `Update/UpdateTaxon.Endpoint.cs`, `Restore/RestoreTaxon.Endpoint.cs`, `Reposition/RepositionTaxon.Endpoint.cs`, `Get/Paged/GetTaxonsAllOrPaged.Endpoint.cs`, `Get/ById/GetTaxonById.Endpoint.cs`, `Get/Tree/GetTaxonTree.Endpoint.cs`, `Get/List/GetTaxonList.Endpoint.cs`) replace `CatalogFeature.Admin.Taxonomies.Taxons.X` with `CatalogFeature.Admin.Taxons.X` (X = Create, Delete, Update, Restore, Reposition, GetAll, GetById, GetTree, GetList). `GetTaxonTree` keeps binding `Guid taxonomyId` from the query string (unchanged). | |  |

### Implementation Phase 3

- GOAL-003: Relocate TaxonRules to `api/admin/catalog/taxon-rules` and move the taxon parent id out of the URL (body for commands, query for the GET).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Create/CreateTaxonRule.Endpoint.cs`: replace `CatalogFeature.Admin.Taxonomies.Taxons.Rules.Create.*` with `CatalogFeature.Admin.TaxonRules.Create.*`; remove the `Guid id` lambda parameter; change construction to `new Command(request.TaxonId, request)`. In `CreateTaxonRule.Request.cs` add `public Guid TaxonId { get; init; }`. Update the `/// Route:` doc comment to `api/admin/catalog/taxon-rules`. In `CreateTaxonRule.Validator.cs` add rule `RuleFor(x => x.TaxonId).NotEmpty()` if a validator exists. | |  |
| TASK-005 | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Get/GetTaxonRules.Endpoint.cs`: replace `...Rules.GetAll.*` with `CatalogFeature.Admin.TaxonRules.GetAll.*`; change the lambda param from `Guid id` to `[FromQuery] Guid taxonId`; keep `new Query(taxonId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/taxon-rules`. | |  |
| TASK-006 | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Update/UpdateTaxonRule.Endpoint.cs`: replace `...Rules.Update.*` with `CatalogFeature.Admin.TaxonRules.Update.*`; change the lambda to `([FromRoute] Guid ruleId, [FromBody] Request request, ...)` and construction to `new Command(request.TaxonId, ruleId, request)`. In `UpdateTaxonRule.Request.cs` add `public Guid TaxonId { get; init; }`. Update the `/// Route:` doc comment to `api/admin/catalog/taxon-rules/{ruleId:guid}`. | |  |
| TASK-007 | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Delete/DeleteTaxonRule.Endpoint.cs`: replace `...Rules.Delete.*` with `CatalogFeature.Admin.TaxonRules.Delete.*`; create `DeleteTaxonRule.Request.cs` in the same folder: `namespace Module.Catalog.Features.Admin.Taxons.Rules.Delete; public static partial class DeleteTaxonRule { public sealed record Request { public Guid TaxonId { get; init; } public Guid RuleId { get; init; } } }`; change the lambda to `([FromRoute] Guid ruleId, [FromBody] Request request, ...)` and construction to `new Command(request.TaxonId, ruleId)`. Update the `/// Route:` doc comment to `api/admin/catalog/taxon-rules/{ruleId:guid}`. | |  |
| TASK-008 | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.Endpoint.cs`: replace `...Rules.Sync.*` with `CatalogFeature.Admin.TaxonRules.Sync.*`; remove the `Guid id` lambda parameter; change construction to `new Command(request.TaxonId, request)`. In `SyncTaxonRules.Request.cs` add `public Guid TaxonId { get; init; }` to the outer `Request` record. Update the `/// Route:` doc comment to `api/admin/catalog/taxon-rules/sync`. | |  |

### Implementation Phase 4

- GOAL-004: Relocate ProductOptionTypes and ProductClassifications to their own top-level groups and move the product parent id out of the URL (body for commands, query for the GET).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.Endpoint.cs`: replace `CatalogFeature.Admin.Products.OptionTypes.Get.*` with `CatalogFeature.Admin.ProductOptionTypes.Get.*`; change the lambda param from `Guid id` to `[FromQuery] Guid productId`; keep `new Query(productId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/product-option-types`. | |  |
| TASK-010 | The 3 product-option-type command endpoints under `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/`: `Assign/AssignProductOptionTypes.Endpoint.cs`, `Revoke/RevokeProductOptionTypes.Endpoint.cs`, `Sync/SyncProductOptionTypes.Endpoint.cs`. Replace `CatalogFeature.Admin.Products.OptionTypes.{Action}.*` with `CatalogFeature.Admin.ProductOptionTypes.{Action}.*` (Action = Assign, Revoke, Sync); remove the `Guid id` lambda parameter; change construction to `new Command(request.ProductId, request)`. Add `public Guid ProductId { get; init; }` to each action `Request` record (`AssignProductOptionTypes.Request.cs`, `RevokeProductOptionTypes.Request.cs`, `SyncProductOptionTypes.Request.cs`). Update `/// Route:` doc comments to `api/admin/catalog/product-option-types/{verb}`. | |  |
| TASK-011 | `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.Endpoint.cs`: replace `CatalogFeature.Admin.Products.Classifications.Get.*` with `CatalogFeature.Admin.ProductClassifications.Get.*`; change the lambda param from `Guid id` to `[FromQuery] Guid productId`; keep `new Query(productId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/product-classifications`. | |  |
| TASK-012 | The 3 product-classification command endpoints under `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/`: `Assign/AssignProductClassifications.Endpoint.cs`, `Revoke/RevokeProductClassifications.Endpoint.cs`, `Sync/SyncProductClassifications.Endpoint.cs`. Replace `CatalogFeature.Admin.Products.Classifications.{Action}.*` with `CatalogFeature.Admin.ProductClassifications.{Action}.*`; remove the `Guid id` lambda parameter; change construction to `new Command(request.ProductId, request)`. Add `public Guid ProductId { get; init; }` to each action `Request` record. Update `/// Route:` doc comments to `api/admin/catalog/product-classifications/{verb}`. | |  |

### Implementation Phase 5

- GOAL-005: Relocate Variants to `api/admin/catalog/variants` (path-only; `Add` already carries `ProductId` in its body).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | In the 5 variant endpoint files under `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/` (`Add/AddVariant.Endpoint.cs`, `Delete/DeleteVariant.Endpoint.cs`, `Update/UpdateVariant.Endpoint.cs`, `Get/ById/GetVariantById.Endpoint.cs`, `Get/PagedOrAll/GetVariantsPagedOrAll.Endpoint.cs`) replace `CatalogFeature.Admin.Products.Variants.X` with `CatalogFeature.Admin.Variants.X` (X = Add, Delete, Update, GetById, GetAll). No route/verb/param changes. Update `/// Route:` doc comments to `api/admin/catalog/variants` (base) or `api/admin/catalog/variants/{id:guid}` (GetById/Update/Delete). | |  |

### Implementation Phase 6

- GOAL-006: Relocate VariantPrices and VariantOptionValues to their own top-level groups and move the variant parent id out of the URL (body for commands, query for the GETs).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Set/SetVariantPrice.Endpoint.cs`: replace `CatalogFeature.Admin.Products.Variants.Prices.Set.*` with `CatalogFeature.Admin.VariantPrices.Set.*`; remove the `[FromRoute] Guid variantId` lambda parameter; change construction to `new Command(request.VariantId, request)`. Add `public Guid VariantId { get; init; }` to the shared `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Shared/Models/Price.Model.Request.cs` (`PriceRequest`), inherited by `SetVariantPrice.Request`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-prices`. | |  |
| TASK-015 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Get/ListPricesByVariant.Endpoint.cs`: replace `...Prices.List.*` with `CatalogFeature.Admin.VariantPrices.List.*`; change the lambda param from `[FromRoute] Guid variantId` to `[FromQuery] Guid variantId`; keep `new Query(variantId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-prices`. | |  |
| TASK-016 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Remove/RemoveVariantPrice.Endpoint.cs`: replace `...Prices.Remove.*` with `CatalogFeature.Admin.VariantPrices.Remove.*`; create `RemoveVariantPrice.Request.cs` in the same folder: `namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Remove; public static partial class RemoveVariantPrice { public sealed record Request { public Guid VariantId { get; init; } public Guid PriceId { get; init; } } }`; change the lambda to `([FromRoute] Guid priceId, [FromBody] Request request, ...)` and construction to `new Command(request.VariantId, priceId)`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-prices/{priceId:guid}`. | |  |
| TASK-017 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Sync/SyncVariantPrices.Endpoint.cs`: replace `...Prices.Sync.*` with `CatalogFeature.Admin.VariantPrices.Sync.*`; remove the `[FromRoute] Guid variantId` lambda parameter; change construction to `new Command(request.VariantId, request)`. Add `public Guid VariantId { get; init; }` to `SyncVariantPrices.Request`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-prices/sync`. | |  |
| TASK-018 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Endpoint.cs`: replace `...OptionValues.Get.*` with `CatalogFeature.Admin.VariantOptionValues.Get.*`; change the lambda param from `Guid variantId` to `[FromQuery] Guid variantId`; keep `new Query(variantId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-option-values`. | |  |
| TASK-019 | The 3 variant-option-value command endpoints under `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/`: `Assign/AssignVariantOptionValues.Endpoint.cs`, `Revoke/RevokeVariantOptionValues.Endpoint.cs`, `Sync/SyncVariantOptionValues.Endpoint.cs`. Replace `...OptionValues.{Action}.*` with `CatalogFeature.Admin.VariantOptionValues.{Action}.*`; remove the `Guid variantId` lambda parameter; change construction to `new Command(request.VariantId, request)`. Add `public Guid VariantId { get; init; }` to each action `Request` record. Update `/// Route:` doc comments to `api/admin/catalog/variant-option-values/{verb}`. | |  |

### Implementation Phase 7

- GOAL-007: Relocate VariantImages and VariantImageEmbeddings to their own top-level groups and move the variant/image parent ids out of the URL.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.Endpoint.cs`: replace `CatalogFeature.Admin.Products.Variants.Images.Upload.*` with `CatalogFeature.Admin.VariantImages.Upload.*`; remove the `[FromRoute] Guid variantId` lambda parameter; change construction to `new Command(request.VariantId, request)`. Add `public Guid VariantId { get; init; }` to the shared upload model under `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Shared/Models/` (`UploadImageRequest`), inherited by `UploadVariantImage.Request`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-images`. | |  |
| TASK-021 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Endpoint.cs`: replace `...Images.GetAll.*` with `CatalogFeature.Admin.VariantImages.GetAll.*`; change the lambda param from `[FromRoute] Guid variantId` to `[FromQuery] Guid variantId`; keep `new Query(variantId, parameters)`. Update the `/// Route:` doc comment to `api/admin/catalog/variant-images`. | |  |
| TASK-022 | The 4 image endpoints that keep `{id:guid}` in the path: `Images/GetById/GetVariantImageById.Endpoint.cs`, `Images/Update/UpdateVariantImage.Endpoint.cs`, `Images/Delete/DeleteVariantImage.Endpoint.cs`, `Images/Download/DownloadVariantImage.Endpoint.cs`. Replace `CatalogFeature.Admin.Products.Variants.Images.{X}.*` with `CatalogFeature.Admin.VariantImages.{X}.*` (X = GetById, Update, Delete, Download). No route/verb/param changes. | |  |
| TASK-023 | The 2 embedding endpoints: `Images/Embeddings/Create/ImageEmbedding.Create.Endpoint.cs` and `Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.Endpoint.cs`. Replace `...Images.Embeddings.Create.*` / `...Images.Embeddings.Regenerate.*` with `CatalogFeature.Admin.VariantImageEmbeddings.Create.*` / `...Regenerate.*`; remove the `[FromRoute] Guid id` lambda parameter; bind `[FromBody] Request request` and construct using `request.VariantImageId` (Create: `new Command(new Request { VariantImageId = request.VariantImageId, ModelName = modelName })`; Regenerate: add `ModelVersion` similarly). Update `/// Route:` doc comments to `api/admin/catalog/variant-image-embeddings` and `api/admin/catalog/variant-image-embeddings/regenerate`. | |  |

### Implementation Phase 8

- GOAL-008: Rewrite `CatalogFeature.Storefront` into the flat shape and update all 11 Storefront endpoint files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Overwrite `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` with the target `Storefront` code in "Reference: Target Flattened Structure". | |  |
| TASK-025 | The 6 Storefront product endpoint files: `Products/Get/Detail/GetProductDetail.Endpoint.cs` (constant -> `Storefront.Products.Detail.*`), `Products/Get/List/ListProducts.Endpoint.cs` (-> `Storefront.Products.List.*`), `Products/SearchByImage/SearchByImage.Endpoint.cs` (-> `Storefront.Products.SearchByImage.*`), `Products/Get/Availability/GetAvailability.Endpoint.cs` (-> `Storefront.Products.Availability.*` + `[FromQuery] Guid productId`), `Products/Get/Related/GetRelatedProducts.Endpoint.cs` (-> `Storefront.Products.Related.*` + `[FromQuery] Guid productId`), `Products/Get/Similar/GetSimilarProducts.Endpoint.cs` (-> `Storefront.Products.Similar.*` + `[FromQuery] Guid productId`). For the three `[FromQuery]` cases keep the positional `new Query(<productId>, parameters)`; update `/// Route:` doc comments to the `api/storefront/products/...` target. | |  |
| TASK-026 | The remaining 5 Storefront endpoint files: `Taxonomies/Get/Tree/GetTree.Endpoint.cs` (constant -> `Storefront.Taxonomies.Tree.*`), `Taxons/Get/All/GetAllTaxons.Endpoint.cs` (-> `Storefront.Taxons.All.*`), `Taxons/Get/Products/GetProducts.Endpoint.cs` (-> `Storefront.Taxons.Products.*` + change `[FromRoute] Guid id` to `[FromQuery] Guid taxonId`, keep `new Query(taxonId, parameters)`, update `/// Route:` comment to `api/storefront/taxons/products`), `OptionTypes/Get/All/GetAllOptionTypes.Endpoint.cs` (-> `Storefront.OptionTypes.All.*`), `Images/Get/Image/GetImage.Endpoint.cs` (-> `Storefront.Images.Image.*`). | |  |

### Implementation Phase 9

- GOAL-009: Update integration, smoke, and workflow tests to the flattened URLs and relocated parent ids.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Update the 5 OptionValues integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/`: replace the URL prefix `api/admin/catalog/option-types/option-values` with `api/admin/catalog/option-values` in every request. | |  |
| TASK-028 | Update the 9 Taxon integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/`: replace the URL prefix `api/admin/catalog/taxonomies/taxons` with `api/admin/catalog/taxons` (keep the `/{id}`/`/tree`/`/list`/`/restore`/`/reposition` suffixes). | |  |
| TASK-029 | Update the 3 ProductOptionTypes integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/OptionTypes/`: `Assign`, `Revoke`, `Get`. Replace `api/admin/catalog/products/{id}/option-types/...` with `api/admin/catalog/product-option-types/...`; for the GET move the product id to the `productId` query param; for Assign/Revoke add `"productId": <id>` to the JSON body. | |  |
| TASK-030 | Update the 3 ProductClassifications integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Classifications/`: `Assign`, `Revoke`, `Get`. Replace `api/admin/catalog/products/{id}/classifications/...` with `api/admin/catalog/product-classifications/...`; GET uses `productId` query param; Assign/Revoke add `"productId": <id>` to the JSON body. | |  |
| TASK-031 | Update the 3 VariantPrices integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Prices/` (`Set/SetPrice.IntegrationTests.cs`, `List/ListPrices.IntegrationTests.cs`, `Remove/RemovePrice.IntegrationTests.cs`): replace `api/admin/catalog/variants/{id}/prices` with `api/admin/catalog/variant-prices`; the GET uses `variantId` query param; Set/Sync add `"variantId": <id>` to the JSON body; Remove uses `api/admin/catalog/variant-prices/{priceId}` with `"variantId": <id>` in the body. | |  |
| TASK-032 | Update the 3 VariantOptionValues integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/OptionValues/` (`Get`, `Assign`, `Revoke`): replace `api/admin/catalog/variants/{id}/option-values` with `api/admin/catalog/variant-option-values`; GET uses `variantId` query param; Assign/Revoke add `"variantId": <id>` to the JSON body. | |  |
| TASK-033 | Update the 6 VariantImages integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/` (`Upload`, `GetAll`, `GetById`, `Update`, `Delete`, `Download`): replace `api/admin/catalog/variants/{id}/images` with `api/admin/catalog/variant-images`; GET uses `variantId` query param; Upload adds `VariantId` as a multipart form field. | |  |
| TASK-034 | Update the 4 Storefront integration tests: `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/Availability/GetProductAvailability.IntegrationTests.cs`, `.../Products/Related/GetRelatedProducts.IntegrationTests.cs`, `.../Products/Similar/GetSimilarProducts.IntegrationTests.cs`, `.../Taxons/GetProducts/GetTaxonProducts.IntegrationTests.cs`: replace `api/storefront/products/{id}/availability|related|similar` with `api/storefront/products/availability|related|similar?productId=<id>` and `api/storefront/taxons/{id}/products` with `api/storefront/taxons/products?taxonId=<id>`. | |  |
| TASK-035 | Update the smoke-test `.http` files under `service/Api/tests/Api.SmokeTests/Catalog/Admin/` (`option-values.http`, `variants.http`, `variant-prices.http`, `variant-option-values.http`, `variant-images.http`, `product-option-types.http`, `product-classifications.http`, `taxons.http`, `taxon-rules.http`) and `service/Api/tests/Api.SmokeTests/Catalog/Storefront/` (`products.http`, `taxonomies-taxons.http`): apply the same URL + parent-id relocations as TASK-027..TASK-034, including updating `#` comment lines above each request. Verify `run-all.http` still references only unchanged routes (`api/admin/catalog/products`, `api/admin/catalog/taxonomies`, `api/admin/catalog/option-types`). | |  |
| TASK-036 | Grep the whole repo for the removed URL path segments and old constant paths: `rg -n "option-types/option-values|taxonomies/taxons|products/variants|/option-types/assign|/classifications/assign|variant-option-values|products/.*/option-types|taxons/.*/rules|Images\.Embeddings|OptionTypes\.OptionValues|Taxonomies\.Taxons\.Rules|Products\.Variants\.Prices|Products\.OptionTypes|Products\.Classifications"`. Fix any remaining references (integration tests, `.http` files, workflow tests under `service/Api/tests/Api.Tests/Scenarios/Workflows/`) to the new flat paths. | |  |

### Implementation Phase 10

- GOAL-010: Verify build, unit tests, convention scripts, and commit.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-037 | Run `dotnet build service/Api/src/Api/Api.csproj --no-restore` — must report `Build succeeded. 0 Warning(s), 0 Error(s)` (CON-001). | |  |
| TASK-038 | Run `dotnet test service/Api/tests/Module.UnitTests --no-build` and `dotnet test service/Api/tests/Shared.UnitTests --no-build` — all unit tests must pass. | |  |
| TASK-039 | Run `bash scripts/check-feature-conventions.sh` and `bash scripts/check-cross-module-refs.sh` — confirm no new violations. | |  |
| TASK-040 | Sweep docs: `rg -ln "option-types/option-values|products/variants|taxonomies/taxons|/rules|/option-types|/classifications|/prices|/embeddings" docs/` and update route references in `docs/` and `plan/` to the new flat URLs (do not rewrite design rationale, only URL strings). Then stage changed files by explicit path and commit with message: `refactor(catalog): flatten CatalogFeature route constants and move nested parent ids out of URLs`. Verify `git status` shows only intended files. | |  |

## 3. Alternatives

- **ALT-001**: Keep URL routes unchanged and flatten only the C# class nesting. **Rejected** — the human confirmed "Flatten URLs too"; the deeply nested paths (`products/variants/{variantId}/prices`) were the primary driver.
- **ALT-002**: Move ALL nested parent ids into query parameters, including for commands. **Rejected** — the human confirmed "Commands: body, Queries: query params"; query-string ids on POST/PUT/DELETE are not RESTful and complicate validation.
- **ALT-003**: Move ALL nested parent ids into the request body, including for GET queries. **Rejected** — GET has no body convention in this stack; query params are the correct transport.
- **ALT-004**: Move feature directories and namespaces to mirror the flattened classes (e.g. `Features/Admin/VariantPrices/Set/`). **Rejected** — it would touch every feature file's namespace and path with no functional gain; the plan intentionally leaves namespaces stable (REQ-005).
- **ALT-005**: Reuse the integration tests' existing partial-flatten routes (`api/admin/catalog/variants/{id}/prices`) as the target. **Rejected** — those tests are internally inconsistent with each other and with production; the parent-id relocation rule supersedes them.

## 4. Dependencies

- **DEP-001**: `CatalogFeatureMetadata` (`service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs`) remains the unchanged permission source of truth referenced by every flattened `Permission` property.
- **DEP-002**: No EF Core model or migration changes are required; only route and request-wiring code changes.
- **DEP-003**: No new NuGet, pnpm, or uv dependencies. `[FromQuery]`/`[FromBody]`/`[FromRoute]`/`[AsParameters]` are available via existing global usings (the current endpoint files already use them without explicit `using Microsoft.AspNetCore.Mvc;`).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` — rewritten to the flat target structure (TASK-001).
- **FILE-002**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` — rewritten to the flat target structure (TASK-024).
- **FILE-003**: 5 OptionValues endpoint files under `Features/Admin/OptionTypes/OptionValues/**` — constant path updates (TASK-002).
- **FILE-004**: 9 Taxon endpoint files under `Features/Admin/Taxonomies/Taxons/**` — constant path updates (TASK-003).
- **FILE-005**: 5 TaxonRules feature files under `Features/Admin/Taxonomies/Taxons/Rules/**` — constant path updates + parent-id relocation, plus `DeleteTaxonRule.Request.cs` created (TASK-004..TASK-008).
- **FILE-006**: 4 ProductOptionTypes feature files under `Features/Admin/Products/OptionTypes/**` — constant path updates + `ProductId` parent-id relocation (TASK-009, TASK-010).
- **FILE-007**: 4 ProductClassifications feature files under `Features/Admin/Products/Classifications/**` — constant path updates + `ProductId` parent-id relocation (TASK-011, TASK-012).
- **FILE-008**: 5 Variant endpoint files under `Features/Admin/Products/Variants/**` (Add/Delete/Update/GetById/GetAll) — constant path updates (TASK-013).
- **FILE-009**: 4 VariantPrices feature files under `Features/Admin/Products/Variants/Prices/**` plus shared `Price.Model.Request.cs` — constant path updates + `VariantId` parent-id relocation, plus `RemoveVariantPrice.Request.cs` created (TASK-014..TASK-017).
- **FILE-010**: 4 VariantOptionValues feature files under `Features/Admin/Products/Variants/OptionValues/**` — constant path updates + `VariantId` parent-id relocation (TASK-018, TASK-019).
- **FILE-011**: 6 VariantImages feature files under `Features/Admin/Products/Variants/Images/**` plus shared upload model — constant path updates + `VariantId` parent-id relocation (TASK-020..TASK-022).
- **FILE-012**: 2 VariantImageEmbeddings feature files under `Features/Admin/Products/Variants/Images/Embeddings/**` — constant path updates + `VariantImageId` relocation (TASK-023).
- **FILE-013**: 11 Storefront endpoint files under `Features/Storefront/**` — constant path updates + parent-id-to-query relocation (TASK-025, TASK-026).
- **FILE-014**: 36 Catalog integration test files under `service/Api/tests/Api.Tests/Scenarios/Catalog/**` — URL and body/query updates (TASK-027..TASK-034).
- **FILE-015**: 11 smoke-test `.http` files under `service/Api/tests/Api.SmokeTests/Catalog/Admin/` and `.../Storefront/` — URL updates (TASK-035).
- **FILE-016**: `docs/` route-reference files — URL string sweep (TASK-040).

## 6. Testing

- **TEST-001**: `dotnet build service/Api/src/Api/Api.csproj --no-restore` returns `0 Warning(s), 0 Error(s)` (TASK-037).
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --no-build` and `dotnet test service/Api/tests/Shared.UnitTests --no-build` all pass (TASK-038).
- **TEST-003**: `bash scripts/check-feature-conventions.sh` reports no new AC-001/002/003/005 violations (TASK-039).
- **TEST-004**: `bash scripts/check-cross-module-refs.sh` reports no new drift (TASK-039).
- **TEST-005**: Grep verification that no removed constant path (`Admin.OptionTypes.OptionValues`, `Admin.Taxonomies.Taxons.Rules`, `Admin.Products.Variants.Prices`, `Storefront.Products.Get.*`, etc.) remains in `service/Api/src/` (TASK-036).
- **TEST-006**: Grep verification that no removed URL segment (`option-types/option-values`, `products/variants`, `taxonomies/taxons`, `/{id}/option-types`, `/{id}/classifications`) remains in tests or docs (TASK-036, TASK-040).

## 7. Risks & Assumptions

- **RISK-001**: This is a breaking HTTP contract change for the Admin and Storefront API surfaces; the Vue SPAs (`app/Admin`, `app/Store`) and any API clients must be updated to the new URLs and relocated parent ids. This plan covers only the backend; frontend migration is a separate follow-up.
- **RISK-002**: Flattened routes with sub-action paths (`api/admin/catalog/taxons/tree`, `api/admin/catalog/variant-prices/sync`) must not collide with `{id:guid}` routes. Mitigation: all `{id}`/`{ruleId}`/`{priceId}` route params keep the `:guid` constraint, so literal segments like `tree`, `list`, `sync`, `assign` never match them.
- **RISK-003**: The existing integration tests under `Api.Tests/Scenarios/Catalog/` are already inconsistent with current production routes (e.g. they call `api/admin/catalog/variants/{id}/prices` while production registers `api/admin/catalog/products/variants/{id}/prices`). Phase 9 rewrites them to the new spec rather than reconciling to the old routes.
- **ASSUMPTION-001**: Feature directories and C# namespaces under `Features/Admin/**` and `Features/Storefront/**` are intentionally unchanged (REQ-005); the plan does not move files.
- **ASSUMPTION-002**: `CatalogFeature.Tags` and `CatalogFeatureMetadata` are out of scope and unchanged (CON-002, CON-003).
- **ASSUMPTION-003**: `AddVariant` already receives `ProductId` via its request body, so the Variants group requires no parent-id relocation.

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — layer responsibilities and feature structure conventions
- `docs/codebase/CONVENTIONS.md` — coding conventions and comment rules
- `.harness/enforcement.yml` — naming, file limits, and import rules
- `docs/superpowers/specs/2026-07-30-catalog-option-types-design.md` — option-type/option-value route history
- `docs/superpowers/specs/2026-07-30-catalog-taxonomies-design.md` — taxonomy/taxon route history
