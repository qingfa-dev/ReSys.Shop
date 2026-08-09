using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    /// <summary>
    /// Represents the command to synchronize roles for a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the full list of role names to assign.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to sync roles for a user.
    /// </summary>
    public sealed class CommandHandler(
        ISystemDateTime dateTime,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IPermissionService permissionService,
        ILogger<CommandHandler> logger
    )
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Synchronizes the role set for a user to match the requested list. Computes the diff
        /// against current roles, validates that requested roles exist, removes departing roles,
        /// adds new roles, persists changes, and invalidates the permission cache.
        /// </summary>
        /// <param name="command">The command with user ID and the full list of target role names.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist role changes.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Retrieve the target user to verify they exist
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Load: Fetch the user's current roles to compute the delta
            var currentRoles = await userManager.GetRolesAsync(user);
            // Compute: Deduplicate the requested role list before diffing
            var requestedRoles = command.Request.Roles.Distinct().ToList();

            // Compute: Roles to add — requested but not currently held
            var rolesToAdd = requestedRoles
                .Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Compute: Roles to remove — currently held but not in the requested set
            var rolesToRemove = currentRoles
                .Where(r => !requestedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToAdd.Count == 0 && rolesToRemove.Count == 0)
                return Result.Ok();

            // Validate: Confirm every role to be added exists in the role store
            foreach (var roleName in rolesToAdd)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    return RoleResult.Failure.NotFound;
                }
            }

            // Call: Remove departing roles first to avoid constraint conflicts
            if (rolesToRemove.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return removeResult.ToResult();
            }

            // Call: Add new roles after removals complete
            if (rolesToAdd.Count > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return addResult.ToResult();
            }

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Call: Persist the updated audit timestamp
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record the sync operation with add/remove counts for audit trail
            UserLoggers.Roles.RolesSynced(logger, UserName: user.UserName!, UserId: user.Id, AddedCount: rolesToAdd.Count,
                RemovedCount: rolesToRemove.Count);

            // Cache: Invalidate the user's permission cache since roles influence effective permissions
            await OnRolesChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnRolesChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}