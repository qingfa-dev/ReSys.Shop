using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Authorization.Registry;

namespace Shared.Security.Authorization.Features;

public static class InventoryFeatureMetadata
{
    public static string ModuleName => "Inventory";

    public static class StockItems
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockItems, PermissionContext.Actions.Read);
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

    public static class StockLocations
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocations, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocations, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocations, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocations, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockLocations, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static class StockReservations
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockReservations, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Cancel = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Inventory, PermissionContext.Resources.StockReservations, PermissionContext.Actions.Cancel);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Cancel];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. StockItems.All,
        .. StockLocations.All,
        .. StockReservations.All,
    ];
}
