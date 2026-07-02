using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Update;

/// <summary>
/// Defines the use case for updating an existing user and their associated profile.
/// </summary>
public static partial class UpdateUser
{
    /// <summary>
    /// Represents the command to update an existing user.
    /// </summary>
    /// <param name="Request">The request containing updated user details.</param>
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to update an existing user and their profile.
    /// </summary>
    public sealed class CommandHandler(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger
    )
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the update of a user account and synchronizes changes to the associated profile.
        /// </summary>
        /// <param name="command">The command with updated user data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the updated user's details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Find the user by its unique identifier.
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Check: Verify if the new email already exists for another user.
            var existingByEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingByEmail is not null && existingByEmail.Id != user.Id)
                return UserResult.Failure.EmailDuplicate;

            // Check: Verify if the new username already exists for another user.
            var existingByUserName = await userManager.FindByNameAsync(request.UserName);
            if (existingByUserName is not null && existingByUserName.Id != user.Id)
                return UserResult.Failure.UsernameDuplicate;

            // Update: Apply changes from the request to the user entity.
            var updateResult = request.MapToDomain(user);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Update: Persist the changes using the user manager.
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful user update
            UserLoggers.Management.Updated(logger, UserName: user.UserName!, Email: user.Email!, UserId: user.Id,
                ActionBy: currentUser.UserName);

            // Map: Convert the updated user entity to the response DTO.
            return user.MapToDetail<Response>();
        }
    }
}