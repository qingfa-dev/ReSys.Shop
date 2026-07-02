using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Roles.Permissions.Sync;

/// <summary>
/// Defines the use case for synchronizing role permissions.
/// </summary>
public static partial class SyncRolePermissions
{
    /// <summary>
    /// Represents the command to synchronize role permissions.
    /// </summary>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to synchronize permissions for a specific role.
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
        /// Handles the command to synchronize permissions for a specific role.
        /// </summary>
        /// <param name="command">The command containing the role ID and target permissions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or failure.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Authentication
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            // Check: Role existence
            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Check: System role protection
            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Validate: Filter requested permissions against the global store of known identifiers
            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(x => x.Identifier).Contains(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Query: Retrieve existing role permissions
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Calculate: Differences
            var permissionsToAdd = requestedPermissions.Except(existingPermissions, StringComparer.OrdinalIgnoreCase).ToList();
            var permissionsToRemove = existingPermissions.Except(requestedPermissions, StringComparer.OrdinalIgnoreCase).ToList();

            if (permissionsToAdd.Count == 0 && permissionsToRemove.Count == 0)
                return Result.Ok();

            // Check: Verify user has authority for all affected permissions (Adds + Removals)
            var affectedPermissions = permissionsToAdd.Concat(permissionsToRemove).ToList();
            var authResult = await permissionService.HasAllPermissionsAsync(currentUserId, affectedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                var userPermsResult = await permissionService.GetEffectiveUserPermissionsAsync(currentUserId, cancellationToken);
                var userPerms = userPermsResult.Value;
                var deniedPermission = affectedPermissions.First(p => !userPerms.Contains(p));

                return RoleResult.Failure.AssignDenied(deniedPermission);
            }

            // Create: Apply batch permission additions
            if (permissionsToAdd.Count > 0)
            {
                var addResult = await permissionService.AddRolePermissionsAsync(role.Id, permissionsToAdd, cancellationToken);
                if (addResult.IsFailure) return addResult;
            }

            // Remove: Apply batch permission removals
            if (permissionsToRemove.Count > 0)
            {
                var removeResult = await permissionService.RemoveRolePermissionsAsync(role.Id, permissionsToRemove, cancellationToken);
                if (removeResult.IsFailure) return removeResult;
            }

            // Update: Record modification
            AuditableBehavior.Touch(role, dateTime.UtcNow);

            // Persist: Save the role state
            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Invalidate: Clear the permission cache for this role.
            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            // Log: Record successful permission sync
            RoleLoggers.Permissions.PermissionsSynced(logger, RoleName: role.Name!, RoleId: role.Id, AddedCount: permissionsToAdd.Count, RemovedCount: permissionsToRemove.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
