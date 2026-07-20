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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Toggles a user's active/inactive status. Blocks self-toggle to prevent accidental lockout,
        /// calls the domain enable/disable method, persists changes, and logs the action.
        /// </summary>
        /// <param name="command">The command with the user ID to toggle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or unauthorized/self-toggle/not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the status change.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Ensure the caller identity is valid before proceeding
            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserResult.Failure.Unauthorized;

            // Guard: Prevent an admin from toggling their own status to avoid accidental lockout
            if (command.Id == currentUserId)
                return UserResult.Failure.SelfStatusToggle;

            // Load: Retrieve the target user to verify they exist
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Call: Apply domain enable/disable logic based on current status
            var updateResult = user.IsActive ? user.Disable() : user.Enable();
            if (updateResult.IsFailure)
                return Result.Validation(errors: updateResult.Errors);

            // Call: Persist the status change via Identity user manager
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult();

            // Log: Record the status toggle with user ID and new state for audit trail
            UserLoggers.Management.StatusToggled(logger, UserId: user.Id, IsActive: user.IsActive, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}