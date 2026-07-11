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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Deletes a user account by ID. Blocks self-deletion. Logs the deletion and returns the deleted user's identity.
        /// </summary>
        /// <param name="command">The command with the user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the deleted user's ID and username, or unauthorized/self-delete/not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the deletion.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (!Guid.TryParse(currentUser.UserId, out var currentUserId))
                return UserResult.Failure.Unauthorized;

            if (request.Id == currentUserId)
                return UserResult.Failure.SelfDelete;

            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            UserLoggers.Management.Deleted(logger,
                UserName: user.UserName!,
                Email: user.Email!,
                UserId: user.Id,
                ActionBy: currentUser.UserName);

            return new Response(user.Id,
                user.UserName ?? string.Empty);
        }
    }
}