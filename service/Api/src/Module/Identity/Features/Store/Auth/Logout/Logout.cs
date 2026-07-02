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
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command to log out the current user.
        /// </summary>
        /// <param name="command">The command containing logout options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating logout success.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            // Check: Ensure user is authenticated
            if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out var userId))
                return UserResult.Failure.InvalidCredentials;

            // Query: Get user entity for logging
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return UserResult.Failure.NotFound;

            // Remove: Revoke all tokens if requested
            if (request.RevokeAll)
            {
                var revokeAllResult = await refreshTokenService.RevokeAllForUserAsync(
                    userId,
                    reason: RefreshTokenConstant.RevocationReasons.UserLogoutAll,
                    cancellationToken);

                if (revokeAllResult.IsFailure)
                    return revokeAllResult.Errors;

                int revokedCount = revokeAllResult.Value;

                // Log: Record logout from all devices
                UserLoggers.Auth.AllDevicesLoggedOut(logger, UserId: userId, DeviceCount: revokedCount, Reason: RefreshTokenConstant.RevocationReasons.UserLogoutAll, ActionBy: user.UserName!);

                return Result.Ok(UserResult.Success.AllDevicesLoggedOut);
            }

            // Remove: Revoke single refresh token
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var revokeResult = await refreshTokenService.RevokeAsync(
                    new RevokeTokenRequestModel(request.RefreshToken, RefreshTokenConstant.RevocationReasons.UserLogout),
                    cancellationToken);

                if (revokeResult.IsFailure)
                    return revokeResult.Errors;

                // Log: Record single device logout
                UserLoggers.Auth.LoggedOut(logger, UserId: userId, Reason: RefreshTokenConstant.RevocationReasons.UserLogout, ActionBy: user.UserName!);
            }

            return Result.Accepted(UserResult.Success.LoggedOut);
        }
    }
}