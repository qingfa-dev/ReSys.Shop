using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Shared.Mappings;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Shared.Admin.Roles.Delete;

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

            // Load: Retrieve the role to verify it exists before deletion
            var role = await roleManager.FindByIdAsync(request.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Guard: Prevent deletion of system-protected roles to maintain platform integrity
            if (role.IsSystem)
            {
                // Log: Record attempted deletion of protected role for security audit
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Call: Execute the deletion via Identity role manager
            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Confirm role was deleted with identifying details
            RoleLoggers.Management.Deleted(logger, RoleName: role.Name!, RoleId: role.Id);

            return role.MapToListItem<Response>();
        }
    }
}