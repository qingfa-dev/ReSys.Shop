using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Roles.Permissions.Revoke;

/// <summary>
/// Defines the use case for revoking permissions from a role.
/// </summary>
public static partial class RevokeRolePermissions
{
    /// <summary>
    /// Represents the command to revoke permissions from a role.
    /// </summary>
    /// <param name="Id">The unique identifier of the role.</param>
    /// <param name="Request">The request containing the list of permission identifiers to revoke.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to revoke permissions from a role.
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
        /// Handles the command to revoke specific permissions from a role.
        /// </summary>
        /// <param name="command">The command with role ID and permissions to revoke.</param>
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

            // Filter: Create a hash set of permission identifiers to revoke for efficient lookup.
            var identifiersToRevoke = command.Request.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (identifiersToRevoke.Count == 0)
                return Result.Ok();

            // Security Check: Verify that the current user has the authority to revoke all requested permissions.
            var authResult = await permissionService.HasAllPermissionsAsync(currentUserId, identifiersToRevoke, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                return RoleResult.Failure.RevokeDenied(identifiersToRevoke.First());
            }

            // Get: Retrieve existing claims (permissions) for the role.
            var existingClaims = await roleManager.GetClaimsAsync(role);
            // Filter: Identify only the claims that match the permissions to be revoked.
            var claimsToRemove = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType && identifiersToRevoke.Contains(c.Value))
                .ToList();

            if (claimsToRemove.Count == 0)
                return Result.Ok();

            // Remove: Execute a batch removal of the permissions from the role.
            var permissionsToRemove = claimsToRemove.Select(c => c.Value).ToList();
            var removeResult = await permissionService.RemoveRolePermissionsAsync(role.Id, permissionsToRemove, cancellationToken);
            if (removeResult.IsFailure)
                return removeResult;

            // Update: Record the modification time for the role.
            AuditableBehavior.Touch(role, dateTime.UtcNow);

            // Sync: Persist the role state.
            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Invalidate: Clear the permission cache for this role.
            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            // Log: Record successful permission revocation
            RoleLoggers.Permissions.PermissionsRevoked(logger, RoleName: role.Name!, RoleId: role.Id, PermissionCount: permissionsToRemove.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
