using Microsoft.AspNetCore.Identity;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Tokens;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Logout;

/// <summary>
/// Defines the use case for user logout.
/// </summary>
public static partial class Logout
{
    // Command
    public record Command(Request Request) : ICommand;
    // CommandHandler
    public class CommandHandler(
        ICurrentUser currentUser,
        IRefreshTokenService refreshTokenService,
        UserManager<User> userManager,
        ILogger<Command> logger)
        : ICommandHandler<Command>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Logs out the current user by revoking their refresh token(s). Supports single-device logout
        /// (one refresh token) or all-devices logout (every token for the user).
        /// </summary>
        /// <param name="command">The command containing logout options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating logout success or unauthorized error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the token store fails to persist revocation.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out var userId))
                return UserResult.Failure.InvalidCredentials;

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            if (request.RevokeAll)
            {
                var revokeAllResult = await refreshTokenService.RevokeAllForUserAsync(
                    userId,
                    reason: RefreshTokenConstant.RevocationReasons.UserLogoutAll,
                    cancellationToken);

                if (revokeAllResult.IsFailure)
                    return revokeAllResult.Errors;

                int revokedCount = revokeAllResult.Value;

                UserLoggers.Auth.AllDevicesLoggedOut(logger, UserId: userId, DeviceCount: revokedCount, Reason: RefreshTokenConstant.RevocationReasons.UserLogoutAll, ActionBy: user.UserName!);

                return Result.Ok(UserResult.Success.AllDevicesLoggedOut);
            }

            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var revokeResult = await refreshTokenService.RevokeAsync(
                    new RevokeTokenRequestModel { Token = request.RefreshToken, Reason = RefreshTokenConstant.RevocationReasons.UserLogout },
                    cancellationToken);

                if (revokeResult.IsFailure)
                    return revokeResult.Errors;

                UserLoggers.Auth.LoggedOut(logger, UserId: userId, Reason: RefreshTokenConstant.RevocationReasons.UserLogout, ActionBy: user.UserName!);
            }

            return Result.Accepted(UserResult.Success.LoggedOut);
        }
    }
}