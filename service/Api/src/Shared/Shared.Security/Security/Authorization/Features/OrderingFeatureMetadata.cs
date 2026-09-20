using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class OrderingFeatureMetadata
{
    public static string ModuleName => "Ordering";

    public static class Orders
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Cancel = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Cancel);
        public static readonly PermissionMetadata Refund = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Refund);
        public static readonly PermissionMetadata ManageItems = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.ManageItems);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Cancel, Refund, ManageItems, Delete];
    }

    public static class Fulfillment
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.Fulfill);
        public static readonly PermissionMetadata Ship = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.Ship);

        public static IReadOnlyList<PermissionMetadata> All => [List, Manage, Ship];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Orders.All,
        .. Fulfillment.All,
    ];
}
