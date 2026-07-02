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
        /// <summary>
        /// Handles the command to revoke direct permissions from a specific user.
        /// </summary>
        /// <param name="command">The command with user ID and permissions to revoke.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or failure due to unauthorized access, user not found, or internal errors.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Ensure the current user is authenticated before proceeding.
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out Guid currentUserId))
                return UserResult.Failure.Unauthorized;

            // Check: Find the target user by their ID.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Filter: Extract and validate permissions from the request against the known PermissionStore.
            var requestedPermissions = command.Request.Permissions
                .Where(p => PermissionContext.All.Select(x => x.Identifier).Contains(p))
                .ToList();

            if (requestedPermissions.Count == 0)
                return Result.Ok();

            // Security Check: Verify that the current user has the authority to revoke all requested permissions.
            var authResult =
                await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                // Note: We return the first one from requested list as 'denied' for simple error reporting.
                return UserResult.Failure.RevokeDenied(requestedPermissions.First());
            }

            // Get: Retrieve existing direct claims (permissions) for the user.
            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Filter: Identify only the permissions that are currently assigned and need to be removed.
            var permissionsToRemove = requestedPermissions
                .Where(p => existingPermissionValues.Contains(p))
                .ToList();

            if (permissionsToRemove.Count == 0)
                return Result.Ok();

            // Remove: Execute a batch removal of the direct permissions from the user.
            var removeResult =
                await permissionService.RemoveUserDirectPermissionsAsync(user.Id, permissionsToRemove,
                    cancellationToken);
            if (removeResult.IsFailure)
                return removeResult;

            // Update: Record the modification time for the user metadata.
            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Sync: Persist the user state.
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record successful permission revocation
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var permissions = string.Join(", ", permissionsToRemove);
                UserLoggers.Permissions.PermissionsRevoked(logger, UserName: user.UserName!, UserId: user.Id,
                    Permissions: permissions, ActionBy: currentUser.UserName);
            }

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