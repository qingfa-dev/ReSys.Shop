using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class PaymentFeatureMetadata
{
    public static string ModuleName => "Payment";

    public static class Payments
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.Payments, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.Payments, PermissionContext.Actions.Manage);
        public static readonly PermissionMetadata Capture = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.Payments, PermissionContext.Actions.Capture);
        public static readonly PermissionMetadata Void = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.Payments, PermissionContext.Actions.Void);
        public static readonly PermissionMetadata Refund = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.Payments, PermissionContext.Actions.Refund);

        public static IReadOnlyList<PermissionMetadata> All => [List, Manage, Capture, Void, Refund];
    }

    public static class PaymentMethods
    {
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Activate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Activate);
        public static readonly PermissionMetadata Deactivate = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Payment, PermissionContext.Resources.PaymentMethods, PermissionContext.Actions.Deactivate);

        public static IReadOnlyList<PermissionMetadata> All => [Create, Read, Update, Delete, Activate, Deactivate];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Payments.All,
        .. PaymentMethods.All,
    ];
}
