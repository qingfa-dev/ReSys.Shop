using Module.Identity.Features.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Permissions.Get;

public static partial class GetRolePermissions
{
    /// <summary>
    /// Represents the response structure for retrieving role permissions.
    /// It organizes permissions by categories and resources, indicating their assignment status.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets or initializes the list of permission categories.
        /// </summary>
        public List<CategoryResponse> Categories { get; init; } = [];

        /// <summary>
        /// Represents a category of permissions.
        /// </summary>
        public sealed record CategoryResponse : CategoryGroupListItemResponse<ResourceResponse>;

        /// <summary>
        /// Represents a resource within a permission category.
        /// </summary>
        public sealed record ResourceResponse : ResourceGroupListItemResponse<PermissionItemResponse>;

        /// <summary>
        /// Represents an individual permission item within a resource, including its assignment status.
        /// </summary>
        public sealed record PermissionItemResponse : PermissionResponse
        {
            /// <summary>
            /// Gets or initializes a value indicating whether this permission is assigned to the role.
            /// </summary>
            public bool IsAssigned { get; init; }
        }
    }
}
