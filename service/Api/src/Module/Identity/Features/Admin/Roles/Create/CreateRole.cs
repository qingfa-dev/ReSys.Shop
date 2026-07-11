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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Creates a new role with a unique name. Validates no duplicate name exists,
        /// persists the role via Identity role manager, and logs the creation.
        /// </summary>
        /// <param name="command">The command containing the role details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the created role's details or an AlreadyExists error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the new role.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var existingRole = await roleManager.FindByNameAsync(request.Name);
            if (existingRole is not null)
                return RoleResult.Failure.AlreadyExists;

            var role = request.MapToDomain();
            role.Id = Guid.NewGuid();

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            RoleLoggers.Management.Created(logger, RoleName: role.Name!, RoleId: role.Id);

            return role.MapToDetail<Response>();
        }
    }
}