using Microsoft.AspNetCore.Identity;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Delete;

/// <summary>
/// Defines the use case for deleting an existing role.
/// </summary>
public static partial class DeleteRole
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        RoleManager<Role> roleManager,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command to delete an existing role.
        /// </summary>
        /// <param name="command">The command containing the role ID to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the deleted role's ID and name or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Attempt to find the role by its ID.
            var role = await roleManager.FindByIdAsync(request.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Check: Prevent deletion of system-protected roles.
            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Remove: Attempt to delete the role from the identity store.
            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful role deletion
            RoleLoggers.Management.Deleted(logger, RoleName: role.Name!, RoleId: role.Id);

            // Map: Return a response with the ID and name of the deleted role.
            return new Response { Id = role.Id, Name = role.Name ?? string.Empty };
        }
    }
}
