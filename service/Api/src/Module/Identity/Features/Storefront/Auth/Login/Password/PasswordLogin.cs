using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Storefront.Shared.Mappings;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Auth.Login.Password;

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
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Authenticates a user via email/phone/username and password, returning JWT and refresh tokens on success.
        /// Validates credentials against Identity, enforces active-status check, and records the login timestamp.
        /// </summary>
        /// <param name="command">The command containing login credentials.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing JWT and refresh tokens or invalid-credentials error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to save the updated login timestamp.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await FindUserByCredentialAsync(request.Credential);
            if (user is null)
                return UserResult.Failure.InvalidCredentials;

            if (!user.IsActive)
                return UserResult.Failure.Inactive;

            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
                return UserResult.Failure.InvalidCredentials;

            var tokenRequest = new TokenRequestModel
            {
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName
            };
            var tokenResult = accessTokenService.GenerateToken(tokenRequest);

            if (tokenResult.IsFailure)
                return tokenResult.Errors;

            var refreshResult = await refreshTokenService.GenerateAsync(user.Id, cancellationToken);

            if (refreshResult.IsFailure)
                return refreshResult.Errors;

            user.LastLoginAtUtc = dateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result.ToResult<Response>();

            UserLoggers.Auth.LoginSucceeded(logger, UserId: user.Id, IpAddress: currentUser.IpAddress, ActionBy: user.UserName!);

            return (tokenResult.Value, refreshResult.Value).MapToTokenResponse<Response>();
        }

        internal async Task<User?> FindUserByCredentialAsync(string credential)
        {
            var user = await userManager.FindByEmailAsync(credential);
            if (user is not null)
                return user;

            user = await userManager.FindByNameAsync(credential);
            if (user is not null)
                return user;

            return await userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber == credential);
        }
    }
}