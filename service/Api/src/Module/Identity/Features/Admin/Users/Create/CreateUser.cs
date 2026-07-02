using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Shared.Mappings;

using Shared.Governance.Conventions;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Create;

/// <summary>
/// Defines the use case for creating a new user.
/// </summary>
public static partial class CreateUser
{
    /// <summary>
    /// Represents the command to create a new user.
    /// </summary>
    /// <param name="Request">The request containing user details.</param>
    public sealed record Command(Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles the <see cref="Command"/> to create a new user.
    /// </summary>
    public sealed class CommandHandler(
        UserManager<User> userManager,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the creation of a new user account.
        /// Checks for duplicate email and username, creates the user without a password,
        /// and emits domain events.
        /// </summary>
        /// <param name="command">The command containing the user data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the created user's details or an error result.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Verify if a user with the same email already exists.
            var existingByEmail = await userManager.FindByEmailAsync(email: request.Email);
            if (existingByEmail is not null)
                return UserResult.Failure.EmailDuplicate;

            // Check: Verify if a user with the same username already exists.
            var existingByUserName = await userManager.FindByNameAsync(userName: request.UserName);
            if (existingByUserName is not null)
                return UserResult.Failure.UsernameDuplicate;

            // Create: Map the request parameters to a new User entity and assign confirmed flags.
            var mapResult = request.MapToDomain();
            if (mapResult.IsFailure) return mapResult.Errors;
            var user = mapResult.Value;

            // Create: Attempt to persist the new user without a password.
            var result = await userManager.CreateAsync(user: user);
            if (!result.Succeeded)
                return result.ToResult<Response>();
            // Generate: Create a password reset token to be used for initial password setup.
            var token = await userManager.GeneratePasswordResetTokenAsync(user: user);
            var setupUrl = BuildSetupPath(userId: user.Id, token: token);

            // Update: Persist the user state.
            var updateResult = await userManager.UpdateAsync(user: user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            // Log: Record successful user creation
            UserLoggers.Management.Created(logger: logger, UserName: user.UserName!, Email: user.Email!, UserId: user.Id);

            // Map: Convert the persisted user entity to the response DTO.
            return user.MapToDetail<Response>();
        }

        private static string BuildSetupPath(Guid userId, string token)
        {
            var encodedToken = token.ToBase64();
            const string path = "setup-password";

            return $"{path}?userId={userId}&token={encodedToken}";
        }
    }
}