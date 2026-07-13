using Microsoft.AspNetCore.Identity;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    /// <summary>
    /// Represents the command to assign roles to a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Request">The request containing the list of role names to assign.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to assign roles to a user.
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
        /// Assigns roles to a user. Validates each role exists and is not already assigned,
        /// persists changes via Identity, and invalidates the user's permission cache
        /// since roles influence effective permissions.
        /// </summary>
        /// <param name="command">The command with user ID and role names to assign.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist role assignments.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var rolesToAdd = new List<string>();
            foreach (var roleName in command.Request.Roles)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    if (!await userManager.IsInRoleAsync(user, roleName))
                    {
                        rolesToAdd.Add(roleName);
                    }
                }
            }

            if (rolesToAdd.Count == 0)
                return Result.Ok();

            var identityResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            AuditableBehavior.Touch(user, dateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                var roles = string.Join(", ", rolesToAdd);
                UserLoggers.Roles.RolesAssigned(logger, UserName: user.UserName!, UserId: user.Id, Roles: roles);
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