using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Roles.Permissions.Assign;

/// <summary>
/// Defines the use case for assigning permissions to a role.
/// </summary>
public static partial class AssignRolePermissions
{
    /// <summary>
    /// Represents the command to assign permissions to a role.
    /// </summary>
    /// <param name="Id">The unique identifier of the role.</param>
    /// <param name="Request">The request containing the list of permission identifiers to assign.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to assign permissions to a role.
    /// </summary>
    public sealed class CommandHandler(
        ISystemDateTime dateTime,
        RoleManager<Role> roleManager,
        IPermissionService permissionService,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command to assign permissions to a specific role.
        /// </summary>
        /// <param name="command">The command with role ID and permissions to assign.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or failure due to unauthorized access, role not found, or system role protection.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the current user is not authenticated.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Ensure the current user is authenticated before proceeding.
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            // Check: Find the role by its ID.
            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Enforce: System roles cannot have their permissions modified dynamically.
            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Filter: Extract and validate permissions from the request against the known PermissionStore.
            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(p => p.Identifier).Contains(p))
                .ToList();

            if (requestedPermissions.Count == 0)
                return Result.Ok();

            // Security Check: Verify that the current user has the authority to assign all requested permissions.
            var authResult =
                await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                // Note: We return the first one from requested list as 'denied' for simple error reporting
                // in a real scenario we might want to find exactly which ones failed.
                return RoleResult.Failure.AssignDenied(requestedPermissions.First());
            }

            // Get: Retrieve existing claims (permissions) for the role.
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Filter: Identify only the new permissions that need to be added.
            var permissionsToAdd = requestedPermissions
                .Where(p => !existingPermissionValues.Contains(p))
                .ToList();

            if (permissionsToAdd.Count == 0)
                return Result.Ok();

            // Add: Execute a batch addition of the new permissions to the role.
            var addResult =
                await permissionService.AddRolePermissionsAsync(role.Id, permissionsToAdd, cancellationToken);
            if (addResult.IsFailure)
                return addResult;

            // Update: Record the modification time for the role.
            AuditableBehavior.Touch(role, dateTime.UtcNow);

            // Sync: Persist the role state.
            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Invalidate: Clear the permission cache for this role.
            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            // Log: Record successful permission assignment
            RoleLoggers.Permissions.PermissionsAssigned(logger, RoleName: role.Name!, RoleId: role.Id,
                PermissionCount: permissionsToAdd.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}