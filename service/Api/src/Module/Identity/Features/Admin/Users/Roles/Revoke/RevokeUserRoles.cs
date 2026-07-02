using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Revoke;

public static partial class RevokeUserRoles
{
    /// <summary>
    /// Represents the command to revoke roles from a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the list of role names to revoke.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to revoke roles from a user.
    /// </summary>
    public sealed class CommandHandler(
        ISystemDateTime dateTime,
        UserManager<User> userManager,
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

            // Filter: Identify roles that the user actually has.
            var rolesToRemove = new List<string>();
            foreach (var roleName in command.Request.Roles)
            {
                if (await userManager.IsInRoleAsync(user, roleName))
                {
                    rolesToRemove.Add(roleName);
                }
            }

            if (rolesToRemove.Count == 0)
                return Result.Ok();

            // Remove: Revoke the roles from the user.
            var identityResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            // Update Metadata:
            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Persist:
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record successful role revocation
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var roles = string.Join(", ", rolesToRemove);
                UserLoggers.Roles.RolesRevoked(logger, UserName: user.UserName!, UserId: user.Id, Roles: roles);
            }

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
