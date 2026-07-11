using Microsoft.Extensions.Options;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers.Facebook.Options;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Authentication.External.Providers.Facebook;

public sealed partial class FacebookExternalProvider(
    IOptions<FacebookOptions> options,
    ILogger<FacebookExternalProvider> logger,
    IFacebookTokenValidator tokenValidator) : IExternalLoginProvider
{
    private readonly FacebookOptions _options = options.Value;
    private readonly ILogger<FacebookExternalProvider> _logger = logger;
    private readonly IFacebookTokenValidator _tokenValidator = tokenValidator;

    public string Provider => "facebook";

    public ProviderOption GetProviderConfig() => new ProviderOption()
    {
        Provider = Provider,
        Options = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId
        }
    };

    public async Task<Result<ExternalUserInfo>> ValidateIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var fbUser = await _tokenValidator.ValidateAsync(idToken, ct);
            if (string.IsNullOrWhiteSpace(fbUser.Email))
                return UserResult.Failure.ExternalLoginEmailMissing;

            var nameParts = (fbUser.Name ?? fbUser.Email.Split('@')[0]).Split(' ', 2);
            return new ExternalUserInfo(
                Provider: Provider,
                ProviderSubjectId: fbUser.Id,
                Email: fbUser.Email,
                FirstName: nameParts[0],
                LastName: nameParts.Length > 1 ? nameParts[1] : null);
        }
        catch (Exception ex)
        {
            Loggers.TokenValidationError(_logger, ex);
            return UserResult.Failure.ExternalLoginTokenInvalid;
        }
    }
}
