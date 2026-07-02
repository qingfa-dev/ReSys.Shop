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
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Ensure the current user is authenticated.
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            // Check: Find the target user.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Filter: Extract and validate permissions from the request.
            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(x => x.Identifier).Contains(p))
                .ToList();

            // Security Check: Verify authority to assign all requested permissions.
            if (requestedPermissions.Count > 0)
            {
                var authResult =
                    await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions,
                        cancellationToken);
                if (authResult.IsFailure || !authResult.Value)
                    return UserResult.Failure.AssignDenied(requestedPermissions.First());
            }

            // Get: Current direct permissions.
            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Calculate Differences:
            var permissionsToAdd = requestedPermissions
                .Where(p => !existingPermissionValues.Contains(p))
                .ToList();

            var permissionsToRemove = existingPermissionValues
                .Where(p => !requestedPermissions.Contains(p, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (permissionsToAdd.Count == 0 && permissionsToRemove.Count == 0)
                return Result.Ok();

            // Security Check for Revocation:
            if (permissionsToRemove.Count > 0)
            {
                var authResult =
                    await permissionService.HasAllPermissionsAsync(currentUserId, permissionsToRemove,
                        cancellationToken);
                if (authResult.IsFailure || !authResult.Value)
                    return UserResult.Failure.RevokeDenied(permissionsToRemove.First());
            }

            // Execute Updates:
            if (permissionsToAdd.Count > 0)
            {
                var addResult =
                    await permissionService.AddUserDirectPermissionsAsync(user.Id, permissionsToAdd, cancellationToken);
                if (addResult.IsFailure) return addResult;
            }

            if (permissionsToRemove.Count > 0)
            {
                var removeResult =
                    await permissionService.RemoveUserDirectPermissionsAsync(user.Id, permissionsToRemove,
                        cancellationToken);
                if (removeResult.IsFailure) return removeResult;
            }

            // Update Metadata:
            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Persist:
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record successful permission sync
            UserLoggers.Permissions.PermissionsSynced(logger, UserName: user.UserName!, UserId: user.Id,
                AddedCount: permissionsToAdd.Count, RemovedCount: permissionsToRemove.Count,
                ActionBy: currentUser.UserName);

            // Post-persist side effects.
            await OnPermissionsChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnPermissionsChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}