using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class DashboardFeatureMetadata
{
    public static string ModuleName => "Dashboard";

    public static class Sales
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.Sales, PermissionContext.Actions.View);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class InventoryDb
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.InventoryDb, PermissionContext.Actions.View);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class CatalogDb
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.CatalogDb, PermissionContext.Actions.View);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class Activity
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.Activity, PermissionContext.Actions.View);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class Logs
    {
        public static readonly PermissionMetadata Audit = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Dashboard, PermissionContext.Resources.Logs, PermissionContext.Actions.Audit);

        public static IReadOnlyList<PermissionMetadata> All => [Audit];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Sales.All,
        .. InventoryDb.All,
        .. CatalogDb.All,
        .. Activity.All,
        .. Logs.All,
    ];
}
