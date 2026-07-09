using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Features;

public static class ProfileFeatureMetadata
{
    public static string ModuleName => "Profile";

    public static class UserProfile
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Profile, PermissionContext.Resources.UserProfile, PermissionContext.Actions.Manage);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Manage];
    }

    public static class NotificationPreferences
    {
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.NotificationPreferences, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.NotificationPreferences, PermissionContext.Actions.Update);

        public static IReadOnlyList<PermissionMetadata> All => [Read, Update];
    }

    public static class Wishlists
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.List);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.Detail);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata AddItem = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.AddItem);
        public static readonly PermissionMetadata RemoveItem = PermissionMetadataMethod.For(
            PermissionContext.Domains.Store, PermissionContext.Categories.Profile, PermissionContext.Resources.Wishlists, PermissionContext.Actions.RemoveItem);

        public static IReadOnlyList<PermissionMetadata> All =>
            [List, Read, Create, Update, Delete, AddItem, RemoveItem];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. UserProfile.All,
        .. NotificationPreferences.All,
        .. Wishlists.All,
    ];
}
