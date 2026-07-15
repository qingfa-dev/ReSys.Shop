namespace Module.Identity.Features.Admin.Roles.Permissions.Assign;

public static partial class AssignRolePermissions
{
    /// <summary>
    /// Represents the request contract for assigning permissions to a role.
    /// </summary>
    // EXCEPTION: feature-specific collection request — no domain entity base
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of permission identifiers to be assigned to the role.
        /// </summary>
        public IEnumerable<string> Permissions { get; init; } = [];
    }
}