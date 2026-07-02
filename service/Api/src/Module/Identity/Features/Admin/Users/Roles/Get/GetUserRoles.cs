using Microsoft.AspNetCore.Identity;

using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    /// <summary>
    /// Represents the query to retrieve all available roles and their assignment status for a specific user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve user roles.
    /// </summary>
    public sealed class QueryHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Find the user.
            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Get: All roles in the system.
            var allRoles = roleManager.Roles.ToList();

            // Get: User's assigned roles.
            var userRoles = await userManager.GetRolesAsync(user);
            var userRolesSet = userRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Map:
            var roles = allRoles.Select(role => new Response.RoleItemResponse
            {
                Name = role.Name!,
                Description = role.Description,
                IsAssigned = userRolesSet.Contains(role.Name!)
            }).ToList();

            return new Response { Roles = roles };
        }
    }
}
