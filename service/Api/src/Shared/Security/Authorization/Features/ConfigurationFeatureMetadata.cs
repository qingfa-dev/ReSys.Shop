using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class ConfigurationFeatureMetadata
{
    public static string ModuleName => "Configuration";

    public static class Settings
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.Settings, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.Settings, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update];
    }

    public static class Stores
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.Stores, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.Stores, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.Stores, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Create, Delete];
    }

    public static class StoreBranding
    {
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.StoreBranding, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Update];
    }

    public static class StoreContact
    {
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.StoreContact, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Update];
    }

    public static class StoreSeo
    {
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.StoreSeo, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Update];
    }

    public static class StoreCurrency
    {
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.StoreCurrency, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Update];
    }

    public static class StoreCheckout
    {
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.StoreCheckout, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Update];
    }

    public static class TaxCategories
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxCategories, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxCategories, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxCategories, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update, Delete];
    }

    public static class TaxRates
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxRates, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxRates, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.TaxRates, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update, Delete];
    }

    public static class CustomFieldDefinitions
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFieldDefinitions, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFieldDefinitions, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFieldDefinitions, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update, Delete];
    }

    public static class CustomFields
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFields, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFields, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Configuration, PermissionContext.Resources.CustomFields, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update, Delete];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Settings.All,
        .. Stores.All,
        .. StoreBranding.All,
        .. StoreContact.All,
        .. StoreSeo.All,
        .. StoreCurrency.All,
        .. StoreCheckout.All,
        .. TaxCategories.All,
        .. TaxRates.All,
        .. CustomFieldDefinitions.All,
        .. CustomFields.All,
    ];
}
