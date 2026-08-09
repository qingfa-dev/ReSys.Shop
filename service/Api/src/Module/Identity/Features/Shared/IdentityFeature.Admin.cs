using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Shared;

public static partial class IdentityFeature
{
    public static class Admin
    {
        public static class Users
        {
            public static class Create
            {
                public const string Route = "api/admin/identity/users";
                public const string Description = "Create a new user";
                public const string Summary = "Create user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Create;
            }

            public static class GetAll
            {
                public const string Route = "api/admin/identity/users";
                public const string Description = "Retrieve all users";
                public const string Summary = "Get all users";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
            }

            public static class GetById
            {
                public const string Route = "api/admin/identity/users/{id:guid}";
                public const string Description = "Retrieve a user by identifier";
                public const string Summary = "Get user by ID";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
            }

            public static class Update
            {
                public const string Route = "api/admin/identity/users/{id:guid}";
                public const string Description = "Update an existing user";
                public const string Summary = "Update user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Update;
            }

            public static class Delete
            {
                public const string Route = "api/admin/identity/users/{id:guid}";
                public const string Description = "Delete a user";
                public const string Summary = "Delete user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Delete;
            }

            public static class Status
            {
                public const string Route = "api/admin/identity/users/{id:guid}/status";
                public const string Description = "Update user active status";
                public const string Summary = "Update user status";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Update;
            }

            public static class Roles
            {
                public static class Get
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/roles";
                    public const string Description = "Retrieve roles assigned to a user";
                    public const string Summary = "Get user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
                }

                public static class Assign
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/roles/assign";
                    public const string Description = "Assign roles to a user";
                    public const string Summary = "Assign user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Assign;
                }

                public static class Revoke
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/roles/revoke";
                    public const string Description = "Revoke roles from a user";
                    public const string Summary = "Revoke user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Revoke;
                }

                public static class Sync
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/roles/sync";
                    public const string Description = "Synchronize user roles";
                    public const string Summary = "Sync user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Sync;
                }
            }

            public static class Permissions
            {
                public static class Get
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/permissions";
                    public const string Description = "Retrieve permissions assigned to a user";
                    public const string Summary = "Get user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
                }

                public static class Assign
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/permissions/assign";
                    public const string Description = "Assign permissions to a user";
                    public const string Summary = "Assign user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Assign;
                }

                public static class Revoke
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/permissions/revoke";
                    public const string Description = "Revoke permissions from a user";
                    public const string Summary = "Revoke user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Revoke;
                }

                public static class Sync
                {
                    public const string Route = "api/admin/identity/users/{id:guid}/permissions/sync";
                    public const string Description = "Synchronize user permissions";
                    public const string Summary = "Sync user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Sync;
                }
            }
        }

        public static class Roles
        {
            public static class Create
            {
                public const string Route = "api/admin/identity/roles";
                public const string Description = "Create a new role";
                public const string Summary = "Create role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Create;
            }

            public static class GetAll
            {
                public const string Route = "api/admin/identity/roles";
                public const string Description = "Retrieve all roles";
                public const string Summary = "Get all roles";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
            }

            public static class GetById
            {
                public const string Route = "api/admin/identity/roles/{id:guid}";
                public const string Description = "Retrieve a role by identifier";
                public const string Summary = "Get role by ID";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
            }

            public static class Update
            {
                public const string Route = "api/admin/identity/roles/{id:guid}";
                public const string Description = "Update an existing role";
                public const string Summary = "Update role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Update;
            }

            public static class Delete
            {
                public const string Route = "api/admin/identity/roles/{id:guid}";
                public const string Description = "Delete a role";
                public const string Summary = "Delete role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Delete;
            }

            public static class Permissions
            {
                public static class Get
                {
                    public const string Route = "api/admin/identity/roles/{id:guid}/permissions";
                    public const string Description = "Retrieve permissions assigned to a role";
                    public const string Summary = "Get role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
                }

                public static class Sync
                {
                    public const string Route = "api/admin/identity/roles/{id:guid}/permissions/sync";
                    public const string Description = "Synchronize role permissions";
                    public const string Summary = "Sync role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Sync;
                }

                public static class Assign
                {
                    public const string Route = "api/admin/identity/roles/{id:guid}/permissions/assign";
                    public const string Description = "Assign permissions to a role";
                    public const string Summary = "Assign role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Assign;
                }

                public static class Revoke
                {
                    public const string Route = "api/admin/identity/roles/{id:guid}/permissions/revoke";
                    public const string Description = "Revoke permissions from a role";
                    public const string Summary = "Revoke role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Revoke;
                }
            }
        }

        public static class Permissions
        {
            public static class Get
            {
                public const string Route = "api/admin/identity/permissions";

                public const string Description =
                    "Retrieves all defined system permissions grouped by category and resource.";

                public const string Summary = "Get all system permissions";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Permissions.List;
            }
        }
    }
}
