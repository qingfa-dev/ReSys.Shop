using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class ShippingFeatureMetadata
{
    public static string ModuleName => "Shipping";

    public static class Methods
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Activate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Activate);
        public static readonly PermissionMetadata Deactivate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingMethods, PermissionContext.Actions.Deactivate);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Activate, Deactivate];
    }

    public static class Rates
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingRates, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingRates, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingRates, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingRates, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Shipping, PermissionContext.Resources.ShippingRates, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static IReadOnlyList<PermissionMetadata> All => [.. Methods.All, .. Rates.All];
}
