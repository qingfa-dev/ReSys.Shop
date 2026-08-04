using Microsoft.AspNetCore.Identity;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Delete;

/// <summary>
/// Defines the use case for deleting an existing user and their associated profile.
/// </summary>
public static partial class DeleteUser
{
    /// <summary>
    /// Represents the command to delete an existing user.
    /// </summary>
    public sealed record Command(Guid Id) : ICommand;

    /// <summary>
    /// Handles the <see cref="Command"/> to delete an existing user and their profile.
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
        /// Deletes a user account by ID. Blocks self-deletion. Logs the deletion and returns success.
        /// </summary>
        /// <param name="command">The command with the user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating success or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the deletion.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = command.Id;

            // Validate: Ensure the caller identity is valid before proceeding
            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserResult.Failure.Unauthorized;

            // Guard: Prevent an admin from deleting their own account
            if (userId == currentUserId)
                return UserResult.Failure.SelfDelete;

            // Load: Retrieve the user to verify they exist before deletion
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Call: Execute the deletion via Identity user manager
            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
                return deleteResult.ToResult();

            // Log: Record user deletion with identifying details for audit trail
            UserLoggers.Management.Deleted(logger,
                UserName: user.UserName!,
                Email: user.Email!,
                UserId: user.Id,
                ActionBy: currentUser.UserName);

            return Result.Ok(UserResult.Success.Deleted);
        }
    }
}