namespace Module.Identity.Features.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    /// <summary>
    /// Represents the request contract for synchronizing a user's roles.
    /// This will replace the user's current roles with the specified list.
    /// </summary>
    public record Request
    {
        /// <summary>
        /// Gets or initializes the complete collection of role names to be assigned to the user.
        /// </summary>
        public IEnumerable<string> Roles { get; init; } = [];
    }
}
