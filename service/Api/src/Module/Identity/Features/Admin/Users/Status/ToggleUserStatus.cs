using Microsoft.AspNetCore.Identity;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Status;

/// <summary>
/// Defines the use case for toggling a user's active status.
/// </summary>
public static partial class ToggleUserStatus
{
    /// <summary>
    /// Represents the command to toggle a user's active status.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    public sealed record Command(Guid Id) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to toggle a user's status.
    /// </summary>
    public sealed class CommandHandler(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger
    )
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Find the user by its unique identifier.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Update: Toggle the active status.
            var updateResult = user.IsActive ? user.Disable() : user.Enable();
            if (updateResult.IsFailure)
                return Result.Validation(errors: updateResult.Errors);

            // Update: Persist the changes.
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult();

            // Log: Record successful status toggle
            UserLoggers.Management.StatusToggled(logger, UserId: user.Id, IsActive: user.IsActive, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}