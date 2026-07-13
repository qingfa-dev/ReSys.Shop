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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Synchronizes the permission set for a non-system role to match the requested list.
        /// Computes the diff (additions and removals) against current claims, validates caller
        /// authority for all affected permissions, applies batch changes, persists the role state,
        /// and invalidates the permission cache.
        /// </summary>
        /// <param name="command">The command containing the role ID and target permissions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/not-found/system-role-protected/assign-denied error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist role claims.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(x => x.Identifier).Contains(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var permissionsToAdd = requestedPermissions.Except(existingPermissions, StringComparer.OrdinalIgnoreCase).ToList();
            var permissionsToRemove = existingPermissions.Except(requestedPermissions, StringComparer.OrdinalIgnoreCase).ToList();

            if (permissionsToAdd.Count == 0 && permissionsToRemove.Count == 0)
                return Result.Ok();

            var affectedPermissions = permissionsToAdd.Concat(permissionsToRemove).ToList();
            var authResult = await permissionService.HasAllPermissionsAsync(currentUserId, affectedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                var userPermsResult = await permissionService.GetEffectiveUserPermissionsAsync(currentUserId, cancellationToken);
                var userPerms = userPermsResult.Value;
                var deniedPermission = affectedPermissions.First(p => !userPerms.Contains(p));

                return RoleResult.Failure.AssignDenied(deniedPermission);
            }

            if (permissionsToAdd.Count > 0)
            {
                var addResult = await permissionService.AddRolePermissionsAsync(role.Id, permissionsToAdd, cancellationToken);
                if (addResult.IsFailure) return addResult;
            }

            if (permissionsToRemove.Count > 0)
            {
                var removeResult = await permissionService.RemoveRolePermissionsAsync(role.Id, permissionsToRemove, cancellationToken);
                if (removeResult.IsFailure) return removeResult;
            }

            AuditableBehavior.Touch(role, dateTime.UtcNow);

            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            RoleLoggers.Permissions.PermissionsSynced(logger, RoleName: role.Name!, RoleId: role.Id, AddedCount: permissionsToAdd.Count, RemovedCount: permissionsToRemove.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}