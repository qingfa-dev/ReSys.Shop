using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Permissions.Revoke;

public static partial class RevokeUserPermissions
{
    /// <summary>
    /// Represents the command to revoke permissions from a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the list of permission identifiers to revoke.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to revoke permissions from a user.
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
        /// Revokes direct permissions from a user. Validates caller authority for each requested permission,
        /// computes intersection with existing claims, persists removals, and invalidates the user's permission cache.
        /// </summary>
        /// <param name="command">The command with user ID and permissions to revoke.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/not-found/revoke-denied error.</returns>
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

            if (requestedPermissions.Count == 0)
                return Result.Ok();

            // Check: Verify the caller holds all requested permissions to prevent unauthorized revocation
            var authResult =
                await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                return UserResult.Failure.RevokeDenied(requestedPermissions.First());
            }

            // Load: Fetch existing permission claims to compute the intersection
            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Compute: Determine which requested permissions are currently assigned
            var permissionsToRemove = requestedPermissions
                .Where(p => existingPermissionValues.Contains(p))
                .ToList();

            if (permissionsToRemove.Count == 0)
                return Result.Ok();

            // Call: Persist removal of permission claims via the permission service
            var removeResult =
                await permissionService.RemoveUserDirectPermissionsAsync(user.Id, permissionsToRemove,
                    cancellationToken);
            if (removeResult.IsFailure)
                return removeResult;

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record permissions revoked with user and caller details for audit trail
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var permissions = string.Join(", ", permissionsToRemove);
                UserLoggers.Permissions.PermissionsRevoked(logger, UserName: user.UserName!, UserId: user.Id,
                    Permissions: permissions, ActionBy: currentUser.UserName);
            }

            // Cache: Invalidate the user's permission cache so revocations take effect immediately
            await OnPermissionsChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnPermissionsChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}