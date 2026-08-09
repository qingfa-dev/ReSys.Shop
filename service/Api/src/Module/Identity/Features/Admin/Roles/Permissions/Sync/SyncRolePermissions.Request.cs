namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Sync;

public static partial class SyncRolePermissions
{
    /// <summary>
    /// Represents the request to synchronize all permissions for a specific role.
    /// </summary>
    // EXCEPTION: feature-specific collection request — no domain entity base
    public record Request
    {
        /// <summary>
        /// Gets the definitive list of permission identifiers the role should have.
        /// Missing permissions will be added; extra permissions will be removed.
        /// </summary>
        public IEnumerable<string> Permissions { get; init; } = [];
    }
}