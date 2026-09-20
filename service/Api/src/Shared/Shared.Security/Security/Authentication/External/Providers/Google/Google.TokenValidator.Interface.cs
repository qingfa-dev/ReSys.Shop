using Google.Apis.Auth;

namespace Shared.Security.Authentication.External.Providers.Google;

public interface IGoogleTokenValidator
{
    Task<GoogleJsonWebSignature.Payload> ValidateAsync(
        string idToken,
        GoogleJsonWebSignature.ValidationSettings settings,
        CancellationToken ct = default);
}