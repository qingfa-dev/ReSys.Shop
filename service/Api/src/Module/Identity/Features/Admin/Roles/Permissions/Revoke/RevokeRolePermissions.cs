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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Revokes permissions from a non-system role. Validates caller authority for each revoked permission,
        /// matches against existing claims, removes only those currently assigned, persists the role state,
        /// and invalidates the permission cache.
        /// </summary>
        /// <param name="command">The command with role ID and permissions to revoke.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/not-found/system-role-protected/revoke-denied error.</returns>
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

            var identifiersToRevoke = command.Request.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (identifiersToRevoke.Count == 0)
                return Result.Ok();

            var authResult = await permissionService.HasAllPermissionsAsync(currentUserId, identifiersToRevoke, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                return RoleResult.Failure.RevokeDenied(identifiersToRevoke.First());
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var claimsToRemove = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType && identifiersToRevoke.Contains(c.Value))
                .ToList();

            if (claimsToRemove.Count == 0)
                return Result.Ok();

            var permissionsToRemove = claimsToRemove.Select(c => c.Value).ToList();
            var removeResult = await permissionService.RemoveRolePermissionsAsync(role.Id, permissionsToRemove, cancellationToken);
            if (removeResult.IsFailure)
                return removeResult;

            AuditableBehavior.Touch(role, dateTime.UtcNow);

            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            RoleLoggers.Permissions.PermissionsRevoked(logger, RoleName: role.Name!, RoleId: role.Id, PermissionCount: permissionsToRemove.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
