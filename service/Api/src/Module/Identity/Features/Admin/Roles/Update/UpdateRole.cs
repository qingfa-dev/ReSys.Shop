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

            var role = await roleManager.FindByIdAsync(command.Id.ToString());
            if (role is null)
                return RoleResult.Failure.NotFound;

            if (role.IsSystem)
            {
                RoleLoggers.Management.SystemRoleProtected(logger, RoleName: role.Name!, RoleId: role.Id);
                return RoleResult.Failure.SystemRoleProtected;
            }

            var existingByName = await roleManager.FindByNameAsync(request.Name);
            if (existingByName is not null && existingByName.Id != role.Id)
                return RoleResult.Failure.AlreadyExists;

            request.MapToDomain(role);
            AuditableBehavior.Touch(role, DateTimeOffset.UtcNow);

            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            RoleLoggers.Management.Updated(logger, RoleName: role.Name!, RoleId: role.Id);

            return role.MapToDetail<Response>();
        }
    }
}