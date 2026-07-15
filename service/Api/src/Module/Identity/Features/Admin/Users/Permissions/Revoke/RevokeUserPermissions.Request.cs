namespace Module.Identity.Features.Admin.Users.Permissions.Revoke;

public static partial class RevokeUserPermissions
{
    /// <summary>
    /// Represents the request contract for revoking direct permissions from a user.
    /// </summary>
    // EXCEPTION: feature-specific collection request — no domain entity base
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of permission identifiers to be revoked from the user.
        /// </summary>
        public IEnumerable<string> Permissions { get; init; } = [];
    }
}