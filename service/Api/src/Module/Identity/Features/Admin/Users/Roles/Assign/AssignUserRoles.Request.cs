namespace Module.Identity.Features.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    /// <summary>
    /// Represents the request contract for assigning roles to a user.
    /// </summary>
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of role names to be assigned to the user.
        /// </summary>
        public IEnumerable<string> Roles { get; init; } = [];
    }
}
