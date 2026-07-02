using Microsoft.AspNetCore.Identity;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Permissions.Get;

public static partial class GetUserPermissions
{
    /// <summary>
    /// Represents the query to retrieve all available permissions and their assignment status (direct and inherited) for a specific user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve user permissions.
    /// </summary>
    public sealed class QueryHandler(
        UserManager<User> userManager,
        IPermissionService permissionService)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the query to get permissions for a specific user, indicating which are direct and which are inherited.
        /// </summary>
        /// <param name="request">The query containing the user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the permission tree with assignment status, or an error if the user is not found.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check: Find the user by its ID.
            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Get: Retrieve the effective permissions for the user via the IPermissionService
            var permissionsResult = await permissionService.GetEffectiveUserPermissionsAsync(user.Id, cancellationToken);
            if (permissionsResult.IsFailure)
                return permissionsResult.Errors;

            var effectivePermissions = permissionsResult.Value;

            // Map: Transform the PermissionContext discovery tree into the response format.
            var categories = PermissionContext.All
                .GroupBy(p => p.Category)
                .Select(categoryGroup => new Response.CategoryResponse
                {
                    Category = categoryGroup.Key,
                    Resources = [.. categoryGroup
                        .GroupBy(p => p.Resource)
                        .Select(resourceGroup => new Response.ResourceResponse
                        {
                            Resource = resourceGroup.Key,
                            Permissions = [.. resourceGroup.Select(permission => new Response.PermissionItemResponse
                            {
                                Identifier = permission.Identifier,
                                Name = permission.Name,
                                Description = permission.Description,
                                Action = permission.Action,
                                IsAssigned = effectivePermissions.Contains(permission.Identifier)
                            })]
                        })]
                })
                .ToList();

            return new Response { Categories = categories };
        }
    }
}
