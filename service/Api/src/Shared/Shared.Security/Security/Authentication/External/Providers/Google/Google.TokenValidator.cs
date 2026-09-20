using Google.Apis.Auth;

namespace Shared.Security.Authentication.External.Providers.Google;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken, GoogleJsonWebSignature.ValidationSettings settings, CancellationToken ct = default)
    {
        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
}
