using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Roles.Shared.Mappings;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Shared.Admin.Roles.Update;

/// <summary>
/// Defines the use case for updating an existing role.
/// </summary>
public static partial class UpdateRole
{
    /// <summary>
    /// Represents the command to update an existing role.
    /// </summary>
    /// <param name="Request">The request containing role update details.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to update an existing role.
    /// </summary>
    public sealed class CommandHandler(
        RoleManager<Role> roleManager,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Updates a role's name and description. Enforces uniqueness of the new name,
        /// blocks updates to system-protected roles, records audit metadata, and logs the change.
        /// </summary>
        /// <param name="command">The command containing the updated role's details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the updated role's details, or NotFound/AlreadyExists/SystemRoleProtected error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the update.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Retrieve the existing role to verify it exists before updating
            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Guard: Prevent modification of system-protected roles to maintain platform integrity
            if (role.IsSystem)
            {
                // Log: Record attempted update of protected role for security audit
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Check: Verify the new name does not conflict with another existing role
            var existingByName = await roleManager.FindByNameAsync(request.Name);
            if (existingByName is not null && existingByName.Id != role.Id)
                return RoleResult.Failure.AlreadyExists;

            // Transform: Apply request data to the existing role entity
            request.MapToDomain(role);
            AuditableBehavior.Touch(role, DateTimeOffset.UtcNow);

            // Call: Persist the updated role via Identity role manager
            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record role update with identifying details for audit trail
            RoleLoggers.Management.Updated(logger, RoleName: role.Name!, RoleId: role.Id);

            // Transform: Return mapped response with updated role details
            return role.MapToDetail<Response>();
        }
    }
}