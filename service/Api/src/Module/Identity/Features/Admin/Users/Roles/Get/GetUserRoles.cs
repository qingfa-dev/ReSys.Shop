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
    /// Handles the <see cref="Query"/> to retrieve user role assignments.
    /// </summary>
    public sealed class QueryHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Retrieves every system role with an assignment flag for the given user.
        /// </summary>
        /// <param name="request">The query containing the user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing all roles with assignment status, or NotFound if the user does not exist.</returns>
        /// <exception cref="DbUpdateException">Thrown when the underlying identity store fails.</exception>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Retrieve the target user to verify they exist
            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Load: Fetch all system-defined roles
            var allRoles = roleManager.Roles.ToList();

            // Load: Retrieve the user's current role assignments
            var userRoles = await userManager.GetRolesAsync(user);
            var userRolesSet = userRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Transform: Build response with each role and its assignment status for the user
            var roles = allRoles.Select(role => new RoleItemResponse
            {
                Name = role.Name!,
                Description = role.Description,
                IsAssigned = userRolesSet.Contains(role.Name!)
            }).ToList();

            return new Response { Roles = roles };
        }
    }
}