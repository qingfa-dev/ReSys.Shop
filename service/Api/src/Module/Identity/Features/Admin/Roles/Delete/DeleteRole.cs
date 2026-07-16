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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Deletes a role by ID. Blocks deletion of system-protected roles.
        /// Logs the deletion and returns the deleted role's identity.
        /// </summary>
        /// <param name="command">The command containing the role ID to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the deleted role's ID and name, or NotFound/SystemRoleProtected error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the deletion.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var role = await roleManager.FindByIdAsync(request.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            RoleLoggers.Management.Deleted(logger, RoleName: role.Name!, RoleId: role.Id);

            // EXCEPTION: deleted role response — no domain entity after deletion
            return new Response { Id = role.Id, Name = role.Name ?? string.Empty };
        }
    }
}