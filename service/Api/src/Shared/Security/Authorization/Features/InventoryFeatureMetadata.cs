using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class InventoryFeatureMetadata
{
    public static string ModuleName => "Inventory";

    public static class StockItem
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Adjust = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Adjust);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Adjust];
    }

    public static class StockLocation
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocation, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocation, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocation, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocation, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocation, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static class StockReservations
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockReservations, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Cancel = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockReservations, PermissionContext.Actions.Cancel);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Cancel];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. StockItem.All,
        .. StockLocation.All,
        .. StockReservations.All,
    ];
}
