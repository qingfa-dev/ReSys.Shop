using Microsoft.AspNetCore.Identity;

using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Create;

using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Login.External.Authenticate;

/// <summary>
/// Defines the use case for external provider authentication.
/// </summary>
public static partial class ExternalAuthenticate
{
    public record Command(Request Request) : ICommand<Response>;

    public class CommandHandler(
        IEnumerable<IExternalLoginProvider> externalLoginProviders,
        UserManager<User> userManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        ISystemDateTime dateTime,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger,
        IMediator mediator)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Authenticates or registers a user via an external OAuth/OpenID provider.
        /// On first login, creates a user account, assigns the default role, links the provider,
        /// and creates a user profile. On subsequent logins, links the provider if not already linked.
        /// Returns JWT and refresh tokens on success.
        /// </summary>
        /// <param name="command">The command containing provider and ID token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing JWT and refresh tokens or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the identity store fails to save user or token state.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var providerKey = request.Provider.Trim().ToLowerInvariant();
            var provider = externalLoginProviders.FirstOrDefault(p => p.Provider == providerKey);
            if (provider is null)
                return UserResult.Failure.ExternalLoginUnsupportedProvider;

            var validationResult = await provider.ValidateIdTokenAsync(request.IdToken, cancellationToken);
            if (validationResult.IsFailure)
                return UserResult.Failure.InvalidCredentials;

            var userInfo = validationResult.Value;

            var user = await userManager.FindByEmailAsync(userInfo.Email);
            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = userInfo.Email,
                    UserName = GenerateUserName(userInfo.Email),
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAtUtc = dateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return createResult.ToResult<Response>();

                var roleResult = await userManager.AddToRoleAsync(user, RoleConstant.Defaults.User);
                if (!roleResult.Succeeded)
                    return roleResult.ToResult<Response>();

                var addLoginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(
                    loginProvider: userInfo.Provider,
                    providerKey: userInfo.ProviderSubjectId,
                    providerDisplayName: userInfo.Provider));
                if (!addLoginResult.Succeeded)
                    return addLoginResult.ToResult<Response>();

                UserLoggers.ExternalLogin.ExternalUserCreated(logger,
                    UserId: user.Id,
                    Provider: userInfo.Provider,
                    Email: userInfo.Email,
                    ActionBy: user.UserName!);

                var profileResult = await CreateUserProfileAsync(user, cancellationToken);
                if (profileResult.IsFailure)
                    return profileResult.Errors;
            }
            else
            {
                var existingLogin = user.UserLogins
                    .FirstOrDefault(l =>
                        l.LoginProvider == userInfo.Provider && l.ProviderKey == userInfo.ProviderSubjectId);

                if (existingLogin is null)
                {
                    var linkResult = await userManager.AddLoginAsync(user, new UserLoginInfo(
                        loginProvider: userInfo.Provider,
                        providerKey: userInfo.ProviderSubjectId,
                        providerDisplayName: userInfo.Provider));

                    if (!linkResult.Succeeded)
                        return linkResult.ToResult<Response>();
                }
            }

            if (!user.IsActive)
                return UserResult.Failure.Inactive;

            var tokenRequest = new TokenRequestModel(user.Id, user.Email!, user.FullName);
            var tokenResult = accessTokenService.GenerateToken(tokenRequest);
            if (tokenResult.IsFailure)
                return tokenResult.Errors;

            var refreshResult = await refreshTokenService.GenerateAsync(user.Id, cancellationToken);
            if (refreshResult.IsFailure)
                return refreshResult.Errors;

            user.LastLoginAtUtc = dateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            UserLoggers.ExternalLogin.ExternalLoginSucceeded(logger,
                UserId: user.Id,
                Provider: userInfo.Provider,
                IpAddress: currentUser.IpAddress,
                ActionBy: user.UserName!);

            return new Response
            {
                AccessToken = tokenResult.Value.Token,
                AccessTokenExpiresIn = tokenResult.Value.ExpiresIn,
                RefreshToken = refreshResult.Value.Token,
                RefreshTokenExpiresIn = new DateTimeOffset(refreshResult.Value.ExpiresAt).ToUnixTimeSeconds()
            };
        }

        private static string GenerateUserName(string email)
        {
            var baseName = email.Split('@')[0].ToLowerInvariant();
            var randomSuffix = Guid.NewGuid().ToString("N")[..6];
            return $"{baseName}_{randomSuffix}";
        }

        private async Task<Result> CreateUserProfileAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var profileResult = await mediator.Send(new CreateProfile.Command(user.Id, new CreateProfile.Request
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email!
                }), cancellationToken);

                if (profileResult.IsFailure)
                {
                    UserProfileLoggers.Management.ProfileCreationFailed(
                        logger, user.Id, string.Join("; ", profileResult.Errors.Select(e => $"{e.Code}: {e.Message}")));
                    return Result.Failure(UserResult.Failure.ProfileCreationFailed);
                }

                UserProfileLoggers.Management.ProfileCreated(logger, user.Id, profileResult.Value.Id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, ex.Message);
                return Result.Failure(UserResult.Failure.ProfileCreationFailed);
            }
        }
    }
}