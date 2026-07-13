namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    /// <summary>
    /// Represents the response structure for retrieving user roles.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets or initializes the list of roles.
        /// </summary>
        public List<RoleItemResponse> Roles { get; init; } = [];

        /// <summary>
        /// Represents an individual role item with its assignment status.
        /// </summary>
        public sealed record RoleItemResponse
        {
            public string Name { get; init; } = default!;
            public string? Description { get; init; }
            public bool IsAssigned { get; init; }
        }
    }
}