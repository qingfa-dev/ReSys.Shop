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
    /// <param name="Request">The request containing the identifier of the user to delete.</param>
    public sealed record Command(Request Request) : ICommand;

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
            var request = command.Request;

            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserResult.Failure.Unauthorized;

            if (request.Id == currentUserId)
                return UserResult.Failure.SelfDelete;

            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
                return deleteResult.ToResult();

            UserLoggers.Management.Deleted(logger,
                UserName: user.UserName!,
                Email: user.Email!,
                UserId: user.Id,
                ActionBy: currentUser.UserName);

            return Result.Ok(UserResult.Success.Deleted);
        }
    }
}