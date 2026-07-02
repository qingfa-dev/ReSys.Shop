using Shared.Security.Authorization.Features;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Identity.Domain.Roles;

/// <summary>
/// Contains constant values for the Role domain.
/// </summary>
public static class RoleConstant
{
    /// <summary>
    /// Default role names and related collections.
    /// </summary>
    public static class Defaults
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";

        /// <summary>
        /// A list of all default role names.
        /// </summary>
        public static readonly string[] All = [Admin, Manager, User];
    }

    /// <summary>
    /// Validation constraints for role properties.
    /// </summary>
    public static class Constraints
    {
        public static class Name
        {
            public const int MaxLength = 64;
        }

        public static class Description
        {
            public const int MaxLength = 256;
        }
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Role.Name),
            nameof(Role.Description),
            nameof(Role.CreatedBy),
            nameof(Role.ModifiedBy)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Role.Name),
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Role.Name),
            nameof(Role.Description),
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];
    }

    public static class RolePermissions
    {
        public static readonly IReadOnlyList<PermissionMetadata> Admin = PermissionContext.All;

        public static readonly IReadOnlyList<PermissionMetadata> Manager =
        [
            .. CatalogFeatureMetadata.All.Where(p => p.Action is not "delete" and not "manage" and not "manage_price"),
            .. IdentityFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "assign" or "revoke" or "manage"),
            .. LocationFeatureMetadata.All,
            .. ProfileFeatureMetadata.All,
            .. OrderingFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "update" or "fulfill" or "ship" or "refund" or "cancel"),
            .. InventoryFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "update" or "adjust"),
            .. ConfigurationFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "update"),
            .. PromotionsFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "create" or "update"),
            .. DashboardFeatureMetadata.All.Where(p => p.Action is "view" or "read"),
        ];

        public static readonly IReadOnlyList<PermissionMetadata> User =
        [
            .. CatalogFeatureMetadata.All.Where(p => p.Action is "view" or "read"),
            .. ProfileFeatureMetadata.All,
            .. OrderingFeatureMetadata.All.Where(p => p.Action is "view" or "read" or "cancel"),
        ];
    }
}
