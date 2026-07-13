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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Revokes roles from a user. Filters to roles the user currently has, removes them
        /// via Identity, persists changes, and invalidates the user's permission cache.
        /// </summary>
        /// <param name="command">The command with user ID and role names to revoke.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist role changes.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

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

            var identityResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                var roles = string.Join(", ", rolesToRemove);
                UserLoggers.Roles.RolesRevoked(logger, UserName: user.UserName!, UserId: user.Id, Roles: roles);
            }

            await OnRolesChangedAsync(user, cancellationToken);

            return Result.Ok();
        }

        private async Task OnRolesChangedAsync(User user, CancellationToken ct)
        {
            await permissionService.InvalidateUserPermissionsAsync(user.Id, ct);
        }
    }
}