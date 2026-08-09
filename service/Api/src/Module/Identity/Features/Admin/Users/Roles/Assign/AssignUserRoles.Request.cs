namespace Module.Identity.Features.Shared.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    // EXCEPTION: feature-specific collection request — no domain entity base
    public record Request
    {
        /// <summary>
        /// Gets or initializes the collection of role names to be assigned to the user.
        /// </summary>
        public IEnumerable<string> Roles { get; init; } = [];
    }
}