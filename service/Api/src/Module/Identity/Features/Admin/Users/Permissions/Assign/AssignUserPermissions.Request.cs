namespace Module.Identity.Features.Admin.Users.Permissions.Assign;

public static partial class AssignUserPermissions
{
    /// <summary>
    /// Represents the request contract for assigning direct permissions to a user.
    /// </summary>
    // EXCEPTION: feature-specific collection request — no domain entity base
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of permission identifiers to be assigned to the user.
        /// </summary>
        public IEnumerable<string> Permissions { get; init; } = [];
    }
}