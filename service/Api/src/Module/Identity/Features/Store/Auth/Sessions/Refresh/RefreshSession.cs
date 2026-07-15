using Microsoft.AspNetCore.Identity;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Sessions.Refresh;

/// <summary>
/// Defines the use case for refreshing an authentication session.
/// </summary>
public static partial class RefreshSession
{
    public record Command(Request Request) : ICommand<Response>;

    public class CommandHandler(
        UserManager<User> userManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService)
            : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Rotates a refresh token and issues a new JWT access token. Validates the token, resolves the
        /// associated user, checks active status, and returns fresh credentials.
        /// </summary>
        /// <param name="command">The command containing the refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the new access and refresh tokens, or token-required/not-found/inactive error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the token store fails to persist rotation.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return UserResult.Failure.TokenRequired;

            var rotateResult = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);
            if (rotateResult.IsFailure)
                return rotateResult.Errors;

            var refreshToken = rotateResult.Value;

            var user = await userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            if (!user.IsActive)
                return UserResult.Failure.Inactive;

            var tokenRequest = new TokenRequestModel(
                user.Id,
                user.Email!,
                user.FullName);
            var tokenResult = accessTokenService.GenerateToken(tokenRequest);

            if (tokenResult.IsFailure)
                return tokenResult.Errors;

            // EXCEPTION: auth token response — no domain entity
            return new Response
            {
                AccessToken = tokenResult.Value.Token,
                AccessTokenExpiresIn = tokenResult.Value.ExpiresIn,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresIn = new DateTimeOffset(refreshToken.ExpiresAt).ToUnixTimeSeconds()
            };
        }
    }
}