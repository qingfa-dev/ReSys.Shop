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
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Find the target user.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Filter: Validate requested roles exist in the system.
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

            // Add: Assign the new roles to the user.
            var identityResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!identityResult.Succeeded)
                return identityResult.ToResult();

            // Update Metadata:
            AuditableBehavior.Touch(user, dateTime.UtcNow);

            // Persist:
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult();

            // Log: Record successful role assignment
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var roles = string.Join(", ", rolesToAdd);
                UserLoggers.Roles.RolesAssigned(logger, UserName: user.UserName!, UserId: user.Id, Roles: roles);
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
