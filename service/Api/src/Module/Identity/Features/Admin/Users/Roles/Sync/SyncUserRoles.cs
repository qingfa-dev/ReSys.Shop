using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Sync;

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
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Find the target user.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Get: Current roles.
            var currentRoles = await userManager.GetRolesAsync(user);
            var requestedRoles = command.Request.Roles.Distinct().ToList();

            // Calculate Differences:
            var rolesToAdd = requestedRoles
                .Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var rolesToRemove = currentRoles
                .Where(r => !requestedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToAdd.Count == 0 && rolesToRemove.Count == 0)
                return Result.Ok();

            // Validate Roles to Add exist:
            foreach (var roleName in rolesToAdd)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    return RoleResult.Failure.NotFound;
                }
            }

            // Execute Updates:
            if (rolesToRemove.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return removeResult.ToResult();
            }

            if (rolesToAdd.Count > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return addResult.ToResult();
            }

            // Update Metadata:
            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Persist:
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record successful role sync
            UserLoggers.Roles.RolesSynced(logger, UserName: user.UserName!, UserId: user.Id, AddedCount: rolesToAdd.Count,
                RemovedCount: rolesToRemove.Count);

            // Post-persist side effects.
            await OnRolesChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnRolesChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}