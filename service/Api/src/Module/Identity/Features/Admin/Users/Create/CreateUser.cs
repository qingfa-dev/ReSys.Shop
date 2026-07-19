using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Shared.Mappings;

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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Creates a new user account via admin action. Validates email and username uniqueness,
        /// maps request to domain, persists the user, generates a password-setup token, and logs the creation.
        /// </summary>
        /// <param name="command">The command containing the user data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the created user's details or a duplicate/validation error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to persist the new user.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Reject duplicate email to enforce email uniqueness constraint
            var existingByEmail = await userManager.FindByEmailAsync(email: request.Email);
            if (existingByEmail is not null)
                return UserResult.Failure.EmailDuplicate;

            // Check: Reject duplicate username to enforce username uniqueness constraint
            var existingByUserName = await userManager.FindByNameAsync(userName: request.UserName);
            if (existingByUserName is not null)
                return UserResult.Failure.UsernameDuplicate;

            // Transform: Map request to domain entity before persistence
            var mapResult = request.MapToDomain();
            if (mapResult.IsFailure) return mapResult.Errors;
            var user = mapResult.Value;

            // Call: Persist the new user via Identity user manager
            var result = await userManager.CreateAsync(user: user);
            if (!result.Succeeded)
                return result.ToResult<Response>();
            // Call: Generate password-reset token so admin can share the setup link
            var token = await userManager.GeneratePasswordResetTokenAsync(user: user);
            // Transform: Build setup URL with encoded token for password initialization
            var setupUrl = BuildSetupPath(userId: user.Id, token: token);

            // Call: Persist audit timestamp updated by setup-path generation
            var updateResult = await userManager.UpdateAsync(user: user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            // Log: Record user creation for audit trail
            UserLoggers.Management.Created(logger: logger, UserName: user.UserName!, Email: user.Email!, UserId: user.Id);

            // Transform: Return mapped response with created user details
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