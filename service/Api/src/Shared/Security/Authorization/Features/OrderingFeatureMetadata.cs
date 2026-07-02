using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Authorization.Registry;

namespace Shared.Security.Authorization.Features;

public static class OrderingFeatureMetadata
{
    public static string ModuleName => "Ordering";

    public static class Orders
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Orders, PermissionContext.Actions.Read);
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
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.Fulfill);
        public static readonly PermissionMetadata Ship = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Fulfillment, PermissionContext.Actions.Ship);

        public static IReadOnlyList<PermissionMetadata> All => [List, Manage, Ship];
    }

    public static class Payments
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Payments, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Payments, PermissionContext.Actions.Manage);
        public static readonly PermissionMetadata Capture = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Payments, PermissionContext.Actions.Capture);
        public static readonly PermissionMetadata Void = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Payments, PermissionContext.Actions.Void);
        public static readonly PermissionMetadata Refund = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.Payments, PermissionContext.Actions.Refund);

        public static IReadOnlyList<PermissionMetadata> All => [List, Manage, Capture, Void, Refund];
    }

    public static class PaymentMethods
    {
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Activate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Activate);
        public static readonly PermissionMetadata Deactivate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Ordering, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Deactivate);

        public static IReadOnlyList<PermissionMetadata> All => [Create, Read, Update, Delete, Activate, Deactivate];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Orders.All,
        .. Fulfillment.All,
        .. Payments.All,
        .. PaymentMethods.All,
    ];
}
