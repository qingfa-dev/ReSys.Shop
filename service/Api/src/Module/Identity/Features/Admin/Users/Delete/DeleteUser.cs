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
    public sealed record Command(Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to delete an existing user and their profile.
    /// </summary>
    public sealed class CommandHandler(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger
    )
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the deletion of a user account and ensures the associated profile is also removed.
        /// </summary>
        /// <param name="command">The command with the user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the deleted user's ID and username, or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserResult.Failure.Unauthorized;

            if (request.Id == currentUserId)
                return Error.Forbidden("User.Delete.Self", "Cannot delete your own account.");

            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Remove: Attempt to delete the user using the user manager.
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful user deletion
            UserLoggers.Management.Deleted(logger,
                UserName: user.UserName!,
                Email: user.Email!,
                UserId: user.Id,
                ActionBy: currentUser.UserName);

            // Create: Return a response with the details of the deleted user.
            return new Response(user.Id,
                user.UserName ?? string.Empty);
        }
    }
}