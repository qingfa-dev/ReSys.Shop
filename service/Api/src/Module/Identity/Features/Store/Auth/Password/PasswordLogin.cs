using Microsoft.AspNetCore.Identity;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Password;

/// <summary>
/// Defines the use case for password-based authentication.
/// </summary>
public static partial class PasswordLogin
{

    // Command
    public record Command(Request Request) : ICommand<Response>;
    // CommandHandler
    public class CommandHandler(
      ISystemDateTime dateTime,
      SignInManager<User> signInManager,
      UserManager<User> userManager,
      IAccessTokenService accessTokenService,
      IRefreshTokenService refreshTokenService,
      ICurrentUser currentUser,
      ILogger<CommandHandler> logger)
      : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command for password-based authentication.
        /// </summary>
        /// <param name="command">The command containing login credentials.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing JWT and refresh tokens or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Query: Find user by email, phone, or username
            var user = await FindUserByCredentialAsync(request.Credential);
            if (user is null)
                return UserResult.Failure.InvalidCredentials;

            // Check: Verify user password with SignInManager
            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
                return UserResult.Failure.InvalidCredentials;

            // Check: Ensure user account is active
            if (!user.IsActive)
                return UserResult.Failure.Inactive;

            // Create: Generate JWT access token
            var tokenRequest = new TokenRequestModel(
                user.Id,
                user.Email!,
                user.FullName);
            var tokenResult = accessTokenService.GenerateToken(tokenRequest);

            if (tokenResult.IsFailure)
                return tokenResult.Errors;

            // Create: Generate refresh token via service
            var refreshResult = await refreshTokenService.GenerateAsync(user.Id, cancellationToken);

            if (refreshResult.IsFailure)
                return refreshResult.Errors;

            // Update: Record login activity and persist changes
            user.LastLoginAtUtc = dateTime.UtcNow;

            // Persist: Save updated user state
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            // Log: Record successful login
            UserLoggers.Auth.LoginSucceeded(logger, UserId: user.Id, IpAddress: currentUser.IpAddress, ActionBy: user.UserName!);

            // Map: Build the success response with tokens
            return new Response()
            {
                AccessToken = tokenResult.Value.Token,
                AccessTokenExpiresIn = tokenResult.Value.ExpiresIn,
                RefreshToken = refreshResult.Value.Token,
                RefreshTokenExpiresIn = new DateTimeOffset(refreshResult.Value.ExpiresAt).ToUnixTimeSeconds()
            };
        }

        internal Task<User?> FindUserByCredentialAsync(string credential)
        {
            var user = userManager.Users.FirstOrDefault(u =>
                u.Email == credential ||
                u.PhoneNumber == credential ||
                u.UserName == credential);
            return Task.FromResult(user);
        }
    }
}