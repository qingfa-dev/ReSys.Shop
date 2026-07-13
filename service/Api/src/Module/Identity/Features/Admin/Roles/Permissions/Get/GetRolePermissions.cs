using Microsoft.AspNetCore.Identity;

using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Permissions.Get;

/// <summary>
/// Defines the use case for retrieving permissions assigned to a role.
/// </summary>
public static partial class GetRolePermissions
{
    /// <summary>
    /// Represents the query to retrieve all available permissions and their assignment status for a specific role.
    /// </summary>
    /// <param name="Id">The unique identifier of the role.</param>
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve role permissions.
    /// </summary>
    public sealed class QueryHandler(RoleManager<Role> roleManager)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Retrieves the full permission tree for a role, marking each permission as assigned or not.
        /// Static permissions derive from the role's name; dynamic permissions come from identity claims.
        /// </summary>
        /// <param name="request">The query containing the role ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the permission tree with assignment status, or NotFound if the role does not exist.</returns>
        /// <exception cref="DbUpdateException">Thrown when the underlying identity store fails to persist claim queries.</exception>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(request.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            var staticPermissions = (role.Name?.ToLowerInvariant() switch
            {
                "admin" => RoleConstant.RolePermissions.Admin,
                "manager" => RoleConstant.RolePermissions.Manager,
                "user" => RoleConstant.RolePermissions.User,
                _ => []
            }).Select(p => p.Identifier);

            var claims = await roleManager.GetClaimsAsync(role);
            var dynamicPermissions = claims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value);

            var assignedIdentifiers = staticPermissions
                .Concat(dynamicPermissions)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
                                IsAssigned = assignedIdentifiers.Contains(permission.Identifier)
                            })]
                        })]
                })
                .ToList();

            var response = new Response
            {
                Categories = categories
            };

            return response;
        }
    }
}