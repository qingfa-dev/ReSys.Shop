namespace Module.Identity.Features.Admin.Users.Roles.Revoke;

public static partial class RevokeUserRoles
{
    /// <summary>
    /// Represents the request contract for revoking roles from a user.
    /// </summary>
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of role names to be revoked from the user.
        /// </summary>
        public IEnumerable<string> Roles { get; init; } = [];
    }
}
