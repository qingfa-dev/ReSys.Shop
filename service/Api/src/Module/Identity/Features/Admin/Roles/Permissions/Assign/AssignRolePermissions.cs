using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Assign;

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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Assigns permissions to a non-system role. Validates the caller's authority for every
        /// requested permission, compares against existing claims to add only new ones, persists
        /// the role state, and invalidates the permission cache.
        /// </summary>
        /// <param name="command">The command with role ID and permissions to assign.</param>
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
                .Where(p => PermissionContext.All.Select(p => p.Identifier).Contains(p))
                .ToList();

            if (requestedPermissions.Count == 0)
                return Result.Ok();

            var authResult =
                await permissionService.HasAllPermissionsAsync(currentUserId, requestedPermissions, cancellationToken);

            if (authResult.IsFailure || !authResult.Value)
            {
                return RoleResult.Failure.AssignDenied(requestedPermissions.First());
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissionValues = existingClaims
                .Where(c => c.Type == PermissionMetadataConstant.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var permissionsToAdd = requestedPermissions
                .Where(p => !existingPermissionValues.Contains(p))
                .ToList();

            if (permissionsToAdd.Count == 0)
                return Result.Ok();

            var addResult =
                await permissionService.AddRolePermissionsAsync(role.Id, permissionsToAdd, cancellationToken);
            if (addResult.IsFailure)
                return addResult;

            AuditableBehavior.Touch(role, dateTime.UtcNow);

            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            await permissionService.InvalidateRolePermissionsAsync(role.Id, cancellationToken);

            RoleLoggers.Permissions.PermissionsAssigned(logger, RoleName: role.Name!, RoleId: role.Id,
                PermissionCount: permissionsToAdd.Count, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}