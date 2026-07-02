using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Authorization.Registry;

namespace Shared.Security.Authorization.Features;

public static class IdentityFeatureMetadata
{
    public static string ModuleName => "Identity";

    public static class Users
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Users, PermissionContext.Actions.Manage);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Manage];
    }

    public static class Roles
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.View);
        public static readonly PermissionMetadata Read = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.Read);
        public static readonly PermissionMetadata Create = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.Create);
        public static readonly PermissionMetadata Update = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.Update);
        public static readonly PermissionMetadata Delete = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.Delete);
        public static readonly PermissionMetadata Manage = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Roles, PermissionContext.Actions.Manage);

        public static IReadOnlyList<PermissionMetadata> All => [List, Read, Create, Update, Delete, Manage];
    }

    public static class Permissions
    {
        public static readonly PermissionMetadata List = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.Permissions, PermissionContext.Actions.View);

        public static IReadOnlyList<PermissionMetadata> All => [List];
    }

    public static class UsersRoles
    {
        public static readonly PermissionMetadata Assign = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersRoles, PermissionContext.Actions.Assign);
        public static readonly PermissionMetadata Revoke = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersRoles, PermissionContext.Actions.Revoke);
        public static readonly PermissionMetadata Sync = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersRoles, PermissionContext.Actions.Sync);

        public static IReadOnlyList<PermissionMetadata> All => [Assign, Revoke, Sync];
    }

    public static class UsersPermissions
    {
        public static readonly PermissionMetadata Assign = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersPermissions, PermissionContext.Actions.Assign);
        public static readonly PermissionMetadata Revoke = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersPermissions, PermissionContext.Actions.Revoke);
        public static readonly PermissionMetadata Sync = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.UsersPermissions, PermissionContext.Actions.Sync);

        public static IReadOnlyList<PermissionMetadata> All => [Assign, Revoke, Sync];
    }

    public static class RolesPermissions
    {
        public static readonly PermissionMetadata Assign = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.RolesPermissions, PermissionContext.Actions.Assign);
        public static readonly PermissionMetadata Revoke = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.RolesPermissions, PermissionContext.Actions.Revoke);
        public static readonly PermissionMetadata Sync = PermissionMetadataMethod.For(
            PermissionContext.Domains.Admin, PermissionContext.Categories.Identity, PermissionContext.Resources.RolesPermissions, PermissionContext.Actions.Sync);

        public static IReadOnlyList<PermissionMetadata> All => [Assign, Revoke, Sync];
    }

    public static IReadOnlyList<PermissionMetadata> All =>
    [
        .. Users.All,
        .. Roles.All,
        .. Permissions.All,
        .. UsersRoles.All,
        .. UsersPermissions.All,
        .. RolesPermissions.All,
    ];
}
