using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class PromotionsFeatureMetadata
{
    public static string ModuleName => "Promotions";

    public static class Promotions
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.Promotions, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.Promotions, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.Promotions, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.Promotions, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.Promotions, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete];
    }

    public static class PromotionRules
    {
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionRules, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionRules, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionRules, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Create, Read, Delete];
    }

    public static class PromotionActions
    {
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionActions, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionActions, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Merchandising, PermissionContext.Resources.PromotionActions, PermissionContext.Actions.Delete);

        public static IReadOnlyList<PermissionMetadata> All => [Create, Read, Delete];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Promotions.All,
        .. PromotionRules.All,
        .. PromotionActions.All,
    ];
}
