using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class CatalogFeatureMetadata
{
    public static string ModuleName => "Catalog";

    public static class Products
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata ManageAssets = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.ManageAssets);
        public static readonly PermissionMetadata ManageMetadata = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.ManageMetadata);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Products, PermissionContext.Actions.Manage);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, ManageAssets, ManageMetadata, Manage];
    }

    public static class Variants
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.Manage);
        public static readonly PermissionMetadata ManagePrice = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariants, PermissionContext.Actions.ManagePrice);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Manage, ManagePrice];
    }

    public static class Taxonomies
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Restore = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Restore);
        public static readonly PermissionMetadata Rebuild = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxonomies, PermissionContext.Actions.Rebuild);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Restore, Rebuild];
    }

    public static class Taxons
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Restore = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.Restore);
        public static readonly PermissionMetadata ManageRules = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.Taxons, PermissionContext.Actions.ManageRules);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Restore, ManageRules];
    }

    public static class OptionTypes
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.OptionTypes, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.OptionTypes, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.OptionTypes, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.OptionTypes, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.OptionTypes, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static class PropertyTypes
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.PropertyTypes, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.PropertyTypes, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.PropertyTypes, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.PropertyTypes, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.PropertyTypes, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static class ProductsOptionTypes
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin,
            PermissionContext.Categories.Catalog,
            PermissionContext.Resources.ProductsOptionTypes,
            PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Assign = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin,
            PermissionContext.Categories.Catalog,
            PermissionContext.Resources.ProductsOptionTypes,
            PermissionContext.Actions.Assign);
        public static readonly PermissionMetadata Revoke = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsOptionTypes, PermissionContext.Actions.Revoke);
        public static readonly PermissionMetadata Sync = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsOptionTypes, PermissionContext.Actions.Sync);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Assign, Revoke, Sync];
    }

    public static class ProductsClassifications
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsClassifications, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Assign = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsClassifications, PermissionContext.Actions.Assign);
        public static readonly PermissionMetadata Revoke = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsClassifications, PermissionContext.Actions.Revoke);
        public static readonly PermissionMetadata Sync = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsClassifications, PermissionContext.Actions.Sync);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Assign, Revoke, Sync];
    }

    public static class VariantOptionValues
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsOptionValues, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsOptionValues, PermissionContext.Actions.Manage);

        public static IReadOnlyList<PermissionMetadata> All => [List, Manage];
    }

    public static class VariantImages
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsImages, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsImages, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Upload = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsImages, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsImages, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Catalog, PermissionContext.Resources.ProductsVariantsImages, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Upload, Update, Delete];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Products.All,
        .. Variants.All,
        .. Taxonomies.All,
        .. Taxons.All,
        .. OptionTypes.All,
        .. PropertyTypes.All,
        .. ProductsOptionTypes.All,
        .. ProductsClassifications.All,
        .. VariantOptionValues.All,
        .. VariantImages.All,
    ];
}
