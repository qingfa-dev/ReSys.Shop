namespace Module.Identity.Features.Admin.Users.Permissions.Sync;

public static partial class SyncUserPermissions
{
    /// <summary>
    /// Represents the request contract for synchronizing direct permissions for a user.
    /// This will replace the user's current direct permissions with the specified list.
    /// </summary>
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of permission identifiers to be assigned to the user.
        /// </summary>
        public IEnumerable<string> Permissions { get; init; } = [];
    }
}