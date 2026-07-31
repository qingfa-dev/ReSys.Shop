using Microsoft.AspNetCore.Identity;

using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve user role assignments.
    /// </summary>
    public sealed class PagedQueryHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
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
            var roles = allRoles.Select(role => new Response
            {
                Name = role.Name!,
                Description = role.Description,
                IsAssigned = userRolesSet.Contains(role.Name!)
            }).OrderBy(r => r.Name).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(roles, 1, Math.Max(1, roles.Count), roles.Count)
                : roles.ToPagedResult(pageModel);
        }
    }
}
