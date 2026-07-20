using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Permissions.Sync;

public static partial class SyncUserPermissions
{
    /// <summary>
    /// Represents the command to synchronize direct permissions for a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the full list of permission identifiers to assign.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to sync permissions for a user.
    /// </summary>
    public sealed class CommandHandler(
        ISystemDateTime dateTime,
        UserManager<User> userManager,
        IPermissionService permissionService,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Synchronizes the direct permission set for a user to match the requested list.
        /// Computes the diff against current claims, validates caller authority for both
        /// additions and removals, applies batch changes, persists, and invalidates the cache.
        /// </summary>
        /// <param name="command">The command with user ID and the full list of target permissions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/not-found/assign-denied/revoke-denied error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist user claims.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Ensure the caller is authenticated before proceeding
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            // Load: Retrieve the target user to verify they exist
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Filter: Only consider permissions that are registered in the global permission registry
            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(x => x.Identifier).Contains(p))
                .ToList();

            // Check: Verify the caller holds all requested permissions before computing the diff
            if (requestedPermissions.Count > 0)
            {
                var authResult =
                    await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions,
                        cancellationToken);
                if (authResult.IsFailure || !authResult.Value)
                    return UserResult.Failure.AssignDenied(requestedPermissions.First());
            }

            // Load: Fetch existing permission claims to compute the add and remove deltas
            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Compute: Determine permissions to add (requested but not present)
            var permissionsToAdd = requestedPermissions
                .Where(p => !existingPermissionValues.Contains(p))
                .ToList();

            // Compute: Determine permissions to remove (present but not requested)
            var permissionsToRemove = existingPermissionValues
                .Where(p => !requestedPermissions.Contains(p, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (permissionsToAdd.Count == 0 && permissionsToRemove.Count == 0)
                return Result.Ok();

            // Check: Verify caller authority for each permission that will be removed
            if (permissionsToRemove.Count > 0)
            {
                var authResult =
                    await permissionService.HasAllPermissionsAsync(currentUserId, permissionsToRemove,
                        cancellationToken);
                if (authResult.IsFailure || !authResult.Value)
                    return UserResult.Failure.RevokeDenied(permissionsToRemove.First());
            }

            // Call: Persist new permission additions via the permission service
            if (permissionsToAdd.Count > 0)
            {
                var addResult =
                    await permissionService.AddUserDirectPermissionsAsync(user.Id, permissionsToAdd, cancellationToken);
                if (addResult.IsFailure) return addResult;
            }

            // Call: Persist permission removals via the permission service
            if (permissionsToRemove.Count > 0)
            {
                var removeResult =
                    await permissionService.RemoveUserDirectPermissionsAsync(user.Id, permissionsToRemove,
                        cancellationToken);
                if (removeResult.IsFailure) return removeResult;
            }

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record the sync operation with add/remove counts for audit trail
            UserLoggers.Permissions.PermissionsSynced(logger, UserName: user.UserName!, UserId: user.Id,
                AddedCount: permissionsToAdd.Count, RemovedCount: permissionsToRemove.Count,
                ActionBy: currentUser.UserName);

            // Cache: Invalidate the user's permission cache so the new set takes effect immediately
            await OnPermissionsChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnPermissionsChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}