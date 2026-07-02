using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Shared.Mappings;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Create;

/// <summary>
/// Defines the use case for creating a new role.
/// </summary>
public static partial class CreateRole
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        RoleManager<Role> roleManager,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command to create a new role.
        /// </summary>
        /// <param name="command">The command containing the role details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the created role's details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Verify if a role with the same name already exists to enforce uniqueness.
            var existingRole = await roleManager.FindByNameAsync(request.Name);
            if (existingRole is not null)
                return RoleResult.Failure.AlreadyExists;

            // Create: Map the request to a new Role entity and assign a unique ID.
            var role = request.MapToDomain();
            role.Id = Guid.NewGuid();

            // Persist: Save the new role to the identity store.
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful role creation
            RoleLoggers.Management.Created(logger, RoleName: role.Name!, RoleId: role.Id);

            // Map: Return the created role details as the response.
            return role.MapToDetail<Response>();
        }
    }
}