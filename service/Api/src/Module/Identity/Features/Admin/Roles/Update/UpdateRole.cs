using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Shared.Mappings;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Update;

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
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command to update an existing role, ensuring uniqueness, system role protection, and raising a domain event.
        /// </summary>
        /// <param name="command">The command containing the updated role's details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the updated role's details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Find the role by its ID.
            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            // Enforce: System roles cannot be updated.
            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            // Check: Verify if a role with the updated name already exists and is not the current role.
            var existingByName = await roleManager.FindByNameAsync(request.Name);
            if (existingByName is not null && existingByName.Id != role.Id)
                return RoleResult.Failure.AlreadyExists;

            // Update: Apply changes from the request to the role entity.
            request.MapToDomain(role);
            AuditableBehavior.Touch(role, DateTimeOffset.UtcNow);

            // Update: Persist the updated role in the identity store.
            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful role update
            RoleLoggers.Management.Updated(logger, RoleName: role.Name!, RoleId: role.Id);

            // Map: Convert the updated role entity to the response DTO.
            return role.MapToDetail<Response>();
        }
    }
}
