using MediatR;

using Microsoft.AspNetCore.Identity;

using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Create;

using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;
using Shared.Security.Identity.Domain.Users.Logins;

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
        // Contract: pre=command!=null, post=result!=null
        /// <summary>
        /// Handles the command for external provider authentication.
        /// </summary>
        /// <param name="command">The command containing provider and ID token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing JWT and refresh tokens or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Query: Resolve the matching external login provider
            var providerKey = request.Provider.Trim().ToLowerInvariant();
            var provider = externalLoginProviders.FirstOrDefault(p => p.Provider == providerKey);
            if (provider is null)
                return UserResult.Failure.ExternalLoginUnsupportedProvider;

            // Validate: Verify the ID token with the provider
            var validationResult = await provider.ValidateIdTokenAsync(request.IdToken, cancellationToken);
            if (validationResult.IsFailure)
                return UserResult.Failure.InvalidCredentials;

            var userInfo = validationResult.Value;

            // Query: Look up existing user by email
            var user = await userManager.FindByEmailAsync(userInfo.Email);
            if (user is null)
            {
                // Create: Instantiate a new User entity from external provider data
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

                // Persist: Create user account
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return createResult.ToResult<Response>();

                // Update: Assign default application role
                var roleResult = await userManager.AddToRoleAsync(user, RoleConstant.Defaults.User);
                if (!roleResult.Succeeded)
                    return roleResult.ToResult<Response>();

                // Update: Link the external login to the user account
                var addLoginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(
                    loginProvider: userInfo.Provider,
                    providerKey: userInfo.ProviderSubjectId,
                    providerDisplayName: userInfo.Provider));
                if (!addLoginResult.Succeeded)
                    return addLoginResult.ToResult<Response>();

                // Log: Record external user creation
                UserLoggers.ExternalLogin.ExternalUserCreated(logger,
                    UserId: user.Id,
                    Provider: userInfo.Provider,
                    Email: userInfo.Email,
                    ActionBy: user.UserName!);

                // Create: User profile for the newly created user
                await CreateUserProfileAsync(user, cancellationToken);
            }
            else
            {
                var existingLogin = user.UserLogins
                    .FirstOrDefault(l =>
                        l.LoginProvider == userInfo.Provider && l.ProviderKey == userInfo.ProviderSubjectId);

                if (existingLogin is null)
                {
                    // Update: Link the external login to the existing user account
                    var linkResult = await userManager.AddLoginAsync(user, new UserLoginInfo(
                        loginProvider: userInfo.Provider,
                        providerKey: userInfo.ProviderSubjectId,
                        providerDisplayName: userInfo.Provider));

                    if (!linkResult.Succeeded)
                        return linkResult.ToResult<Response>();
                }
            }

            // Check: Ensure user account is active
            if (!user.IsActive)
                return UserResult.Failure.Inactive;

            // Create: Generate JWT access token
            var tokenRequest = new TokenRequestModel(user.Id, user.Email!, user.FullName);
            var tokenResult = accessTokenService.GenerateToken(tokenRequest);
            if (tokenResult.IsFailure)
                return tokenResult.Errors;

            // Create: Generate refresh token
            var refreshResult = await refreshTokenService.GenerateAsync(user.Id, cancellationToken);
            if (refreshResult.IsFailure)
                return refreshResult.Errors;

            // Update: Record login activity
            user.LastLoginAtUtc = dateTime.UtcNow;
            user.UserLogins.Add(new UserLogin
            {
                LoginProvider = userInfo.Provider,
                ProviderKey = userInfo.ProviderSubjectId,
                ProviderDisplayName = userInfo.Provider,
                UserId = user.Id,
            });

            // Persist: Save updated user state
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.ToResult<Response>();

            // Log: Record successful external login
            UserLoggers.ExternalLogin.ExternalLoginSucceeded(logger,
                UserId: user.Id,
                Provider: userInfo.Provider,
                IpAddress: currentUser.IpAddress,
                ActionBy: user.UserName!);

            // Map: Build the success response with tokens
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

        private async Task CreateUserProfileAsync(User user, CancellationToken cancellationToken)
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
                    var errors = string.Join("; ", profileResult.Errors.Select(e => $"{e.Code}: {e.Message}"));
                    UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, errors);
                }
                else
                {
                    UserProfileLoggers.Management.ProfileCreated(logger, user.Id, profileResult.Value.Id);
                }
            }
            catch (Exception ex)
            {
                UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, ex.Message);
            }
        }
    }
}