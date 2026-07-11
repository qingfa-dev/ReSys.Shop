using Google.Apis.Auth;

using Microsoft.Extensions.Options;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers.Google.Options;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Authentication.External.Providers.Google;

/// <summary>
/// Validates Google OAuth ID tokens via Google.Apis.Auth.
/// Implements IExternalLoginProvider for the "google" provider key.
/// </summary>
public sealed partial class GoogleExternalProvider(
    IOptions<GoogleOptions> options,
    ILogger<GoogleExternalProvider> logger,
    IGoogleTokenValidator tokenValidator) : IExternalLoginProvider
{
    private readonly GoogleOptions _options = options.Value;
    private readonly ILogger<GoogleExternalProvider> _logger = logger;
    private readonly IGoogleTokenValidator _tokenValidator = tokenValidator;

    public string Provider => "google";

    public ProviderOption GetProviderConfig() => new ProviderOption()
    {
        Provider = Provider,
        Options = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId
        }
    };


    /// <summary>
    /// Validates a Google ID token and returns user profile information.
    /// </summary>
    /// <param name="idToken">The Google ID token to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing ExternalUserInfo or an error.</returns>
    /// <exception cref="InvalidJwtException">Thrown when the token signature or payload is invalid.</exception>
    // Contract: pre=!string.IsNullOrEmpty(idToken) && _options.ClientId!=null,
    //           post=result.IsSuccess||result.IsFailure, throws=InvalidJwtException
    public async Task<Result<ExternalUserInfo>> ValidateIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            // Call: Google token validation via GoogleJsonWebSignature.ValidateAsync against configured ClientId
            GoogleJsonWebSignature.Payload payload = await _tokenValidator.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            }, ct);

            // Check: Google returned an email — required for user identification
            if (string.IsNullOrWhiteSpace(payload.Email))
                return UserResult.Failure.ExternalLoginEmailMissing;

            // Create: ExternalUserInfo with provider, subject ID, email, and name claims from Google payload
            return new ExternalUserInfo(
                Provider: Provider,
                ProviderSubjectId: payload.Subject,
                Email: payload.Email,
                FirstName: payload.GivenName ?? payload.Name ?? payload.Email.Split('@')[0],
                LastName: payload.FamilyName);
        }
        // Catch: Invalid JWT — log warning and return ExternalLoginTokenInvalid error
        catch (InvalidJwtException ex)
        {
            // Log: Google ID token validation failure — structured warning with exception details
            Loggers.TokenValidationError(_logger, ex);
            return UserResult.Failure.ExternalLoginTokenInvalid;
        }
    }
}
