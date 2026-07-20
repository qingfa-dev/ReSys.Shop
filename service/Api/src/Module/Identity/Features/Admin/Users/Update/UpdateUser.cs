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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Updates a user's details by ID. Validates uniqueness of the new email and username
        /// (excluding the current user), maps changes to the domain entity, persists via Identity,
        /// and logs the update.
        /// </summary>
        /// <param name="command">The command with updated user data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the updated user's details, or not-found/duplicate error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the update.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Retrieve the target user to verify they exist
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Check: Reject duplicate email if another user already holds it
            var existingByEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingByEmail is not null && existingByEmail.Id != user.Id)
                return UserResult.Failure.EmailDuplicate;

            // Check: Reject duplicate username if another user already holds it
            var existingByUserName = await userManager.FindByNameAsync(request.UserName);
            if (existingByUserName is not null && existingByUserName.Id != user.Id)
                return UserResult.Failure.UsernameDuplicate;

            // Transform: Apply request data to the existing user entity
            var updateResult = request.MapToDomain(user);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Call: Persist the updated user via Identity user manager
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record user update with identifying details for audit trail
            UserLoggers.Management.Updated(logger, UserName: user.UserName!, Email: user.Email!, UserId: user.Id,
                ActionBy: currentUser.UserName);

            // Transform: Return mapped response with updated user details
            return user.MapToDetail<Response>();
        }
    }
}