using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features;

public static class IdentityFeature
{
    public static class Tags
    {
        public static readonly string[] User = ["User"];
        public static readonly string[] Role = ["Role"];
        public static readonly string[] Permission = ["Permission"];

        public static readonly string[] Authentication = ["Authentication"];
        public static readonly string[] Authorization = ["Authorization"];
    }

    public static class Store
    {
        private const string StoreRoute = "api/store/identity";

        public static class Users
        {
            private const string BaseUserRoute = $"{StoreRoute}/users";

            public static class GetAll
            {
                public const string Route = BaseUserRoute;
                public const string Description = "Retrieve users";
                public const string Summary = "Get users";
            }

            public static class GetById
            {
                public const string Route = $"{BaseUserRoute}/{{id:guid}}";
                public const string Description = "Retrieve a user by identifier";
                public const string Summary = "Get user by ID";
            }
        }

        public static class Auth
        {
            private const string BaseAuthRoute = $"{StoreRoute}/auth";

            public static class Login
            {
                private const string BaseLoginRoute = $"{BaseAuthRoute}/login";

                public static class Password
                {
                    public const string Route = $"{BaseLoginRoute}/password";

                    public const string Description =
                        "Authenticate a store user with email/username/phone and password.";

                    public const string Summary = "Password login";
                }

                public static class External
                {
                    private const string BaseExternalRoute = $"{BaseLoginRoute}/external";

                    public static class Authenticate
                    {
                        public const string Route = BaseExternalRoute;

                        public const string Description =
                            "Authenticate a store user using an external provider (Google, etc.) via ID token.";

                        public const string Summary = "External login";
                    }

                    public static class Providers
                    {
                        public const string Route = $"{BaseExternalRoute}/providers";
                        public const string Description = "Retrieve configured external login provider options.";
                        public const string Summary = "External providers";
                    }
                }
            }

            public static class Register
            {
                public const string Route = $"{BaseAuthRoute}/register";
                public const string Description = "Register a new user account via email.";
                public const string Summary = "Email register";
            }

            public static class Logout
            {
                public const string Route = $"{BaseAuthRoute}/logout";
                public const string Description = "Logout a store user from current device or all devices.";
                public const string Summary = "Logout";
            }

            public static class Sessions
            {
                private const string BaseRoute = $"{BaseAuthRoute}/sessions";

                public static class Get
                {
                    public const string Route = BaseRoute;
                    public const string Description = "Retrieve the current authenticated user's session.";
                    public const string Summary = "Get session";
                }

                public static class Refresh
                {
                    public const string Route = $"{BaseRoute}/refresh";
                    public const string Description = "Refresh an expired access token using a valid refresh token.";
                    public const string Summary = "Refresh session";
                }
            }
        }

        public static class Roles
        {
            private const string BaseRoleRoute = $"{StoreRoute}/roles";

            public static class GetAll
            {
                public const string Route = BaseRoleRoute;
                public const string Description = "Retrieve roles";
                public const string Summary = "Get roles";
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoleRoute}/{{id:guid}}";
                public const string Description = "Retrieve a role by identifier";
                public const string Summary = "Get role by ID";
            }
        }

        public static class Phones
        {
            private const string BasePhoneRoute = $"{StoreRoute}/phones";

            public static class Change
            {
                public const string Route = $"{BasePhoneRoute}/change";
                public const string Description = "Request phone number change";
                public const string Summary = "Change phone";
            }

            public static class Confirm
            {
                public const string Route = $"{BasePhoneRoute}/confirm";
                public const string Description = "Confirm phone number change";
                public const string Summary = "Confirm phone";
            }

            public static class Resend
            {
                public const string Route = $"{BasePhoneRoute}/resend";
                public const string Description = "Resend phone verification code";
                public const string Summary = "Resend verification";
            }
        }

        public static class Passwords
        {
            private const string BasePasswordRoute = $"{StoreRoute}/passwords";

            public static class Change
            {
                public const string Route = $"{BasePasswordRoute}/change";
                public const string Description = "Change user password";
                public const string Summary = "Change password";
            }

            public static class Forgot
            {
                public const string Route = $"{BasePasswordRoute}/forgot";
                public const string Description = "Request password reset";
                public const string Summary = "Forgot password";
            }

            public static class Reset
            {
                public const string Route = $"{BasePasswordRoute}/reset";
                public const string Description = "Reset user password";
                public const string Summary = "Reset password";
            }
        }

        public static class Emails
        {
            private const string BaseEmailRoute = $"{StoreRoute}/emails";

            public static class Change
            {
                public const string Route = $"{BaseEmailRoute}/change";
                public const string Description = "Request email change";
                public const string Summary = "Change email";
            }

            public static class Confirm
            {
                public const string Route = $"{BaseEmailRoute}/confirm";
                public const string Description = "Confirm email verification";
                public const string Summary = "Confirm email";
            }

            public static class Resend
            {
                public const string Route = $"{BaseEmailRoute}/resend";
                public const string Description = "Resend email verification";
                public const string Summary = "Resend verification";
            }
        }
    }

    public static class Admin
    {
        private const string AdminRoute = "api/identity";

        public static class Users
        {
            private const string BaseUserRoute = $"{AdminRoute}/users";

            public static class Create
            {
                public const string Route = BaseUserRoute;
                public const string Description = "Create a new user";
                public const string Summary = "Create user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseUserRoute;
                public const string Description = "Retrieve all users";
                public const string Summary = "Get all users";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseUserRoute}/{{id:guid}}";
                public const string Description = "Retrieve a user by identifier";
                public const string Summary = "Get user by ID";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseUserRoute}/{{id:guid}}";
                public const string Description = "Update an existing user";
                public const string Summary = "Update user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseUserRoute}/{{id:guid}}";
                public const string Description = "Delete a user";
                public const string Summary = "Delete user";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Delete;
            }

            public static class Status
            {
                public const string Route = $"{BaseUserRoute}/{{id:guid}}/status";
                public const string Description = "Update user active status";
                public const string Summary = "Update user status";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.Update;
            }

            public static class Roles
            {
                private const string RoleBaseRoute = $"{BaseUserRoute}/{{id:guid}}/roles";

                public static class Get
                {
                    public const string Route = RoleBaseRoute;
                    public const string Description = "Retrieve roles assigned to a user";
                    public const string Summary = "Get user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
                }

                public static class Assign
                {
                    public const string Route = $"{RoleBaseRoute}/assign";
                    public const string Description = "Assign roles to a user";
                    public const string Summary = "Assign user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Assign;
                }

                public static class Revoke
                {
                    public const string Route = $"{RoleBaseRoute}/revoke";
                    public const string Description = "Revoke roles from a user";
                    public const string Summary = "Revoke user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Revoke;
                }

                public static class Sync
                {
                    public const string Route = $"{RoleBaseRoute}/sync";
                    public const string Description = "Synchronize user roles";
                    public const string Summary = "Sync user roles";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersRoles.Sync;
                }
            }

            public static class Permissions
            {
                private const string PermissionBaseRoute = $"{BaseUserRoute}/{{id:guid}}/permissions";

                public static class Get
                {
                    public const string Route = PermissionBaseRoute;
                    public const string Description = "Retrieve permissions assigned to a user";
                    public const string Summary = "Get user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Users.List;
                }

                public static class Assign
                {
                    public const string Route = $"{PermissionBaseRoute}/assign";
                    public const string Description = "Assign permissions to a user";
                    public const string Summary = "Assign user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Assign;
                }

                public static class Revoke
                {
                    public const string Route = $"{PermissionBaseRoute}/revoke";
                    public const string Description = "Revoke permissions from a user";
                    public const string Summary = "Revoke user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Revoke;
                }

                public static class Sync
                {
                    public const string Route = $"{PermissionBaseRoute}/sync";
                    public const string Description = "Synchronize user permissions";
                    public const string Summary = "Sync user permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.UsersPermissions.Sync;
                }
            }
        }

        public static class Roles
        {
            private const string BaseRoleRoute = $"{AdminRoute}/roles";

            public static class Create
            {
                public const string Route = BaseRoleRoute;
                public const string Description = "Create a new role";
                public const string Summary = "Create role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoleRoute;
                public const string Description = "Retrieve all roles";
                public const string Summary = "Get all roles";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoleRoute}/{{id:guid}}";
                public const string Description = "Retrieve a role by identifier";
                public const string Summary = "Get role by ID";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoleRoute}/{{id:guid}}";
                public const string Description = "Update an existing role";
                public const string Summary = "Update role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoleRoute}/{{id:guid}}";
                public const string Description = "Delete a role";
                public const string Summary = "Delete role";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.Delete;
            }

            public static class Permissions
            {
                private const string PermissionBaseRoute = $"{BaseRoleRoute}/{{id:guid}}/permissions";

                public static class Get
                {
                    public const string Route = PermissionBaseRoute;
                    public const string Description = "Retrieve permissions assigned to a role";
                    public const string Summary = "Get role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.Roles.List;
                }

                public static class Sync
                {
                    public const string Route = $"{PermissionBaseRoute}/sync";
                    public const string Description = "Synchronize role permissions";
                    public const string Summary = "Sync role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Sync;
                }

                public static class Assign
                {
                    public const string Route = $"{PermissionBaseRoute}/assign";
                    public const string Description = "Assign permissions to a role";
                    public const string Summary = "Assign role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Assign;
                }

                public static class Revoke
                {
                    public const string Route = $"{PermissionBaseRoute}/revoke";
                    public const string Description = "Revoke permissions from a role";
                    public const string Summary = "Revoke role permissions";
                    public static PermissionMetadata Permission => IdentityFeatureMetadata.RolesPermissions.Revoke;
                }
            }
        }

        public static class Permissions
        {
            private const string PermissionBaseRoute = $"{AdminRoute}/permissions";

            public static class Get
            {
                public const string Route = PermissionBaseRoute;

                public const string Description =
                    "Retrieves all defined system permissions grouped by category and resource.";

                public const string Summary = "Get all system permissions";
                public static PermissionMetadata Permission => IdentityFeatureMetadata.Permissions.List;
            }
        }
    }
}