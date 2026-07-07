using Microsoft.Extensions.Options;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers.Microsoft.Options;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Authentication.External.Providers.Microsoft;

public sealed class MicrosoftExternalProvider(
    IOptions<MicrosoftOptions> options,
    ILogger<MicrosoftExternalProvider> logger,
    IMicrosoftTokenValidator tokenValidator) : IExternalLoginProvider
{
    private readonly MicrosoftOptions _options = options.Value;
    private readonly ILogger<MicrosoftExternalProvider> _logger = logger;
    private readonly IMicrosoftTokenValidator _tokenValidator = tokenValidator;

    public string Provider => "microsoft";

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
            var msUser = await _tokenValidator.ValidateAsync(idToken, ct);
            if (string.IsNullOrWhiteSpace(msUser.Mail))
                return UserResult.Failure.ExternalLoginEmailMissing;

            var nameParts = (msUser.DisplayName ?? msUser.Mail.Split('@')[0]).Split(' ', 2);
            return new ExternalUserInfo(
                Provider: Provider,
                ProviderSubjectId: msUser.Id,
                Email: msUser.Mail,
                FirstName: nameParts[0],
                LastName: nameParts.Length > 1 ? nameParts[1] : null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Microsoft access token validation failed");
            return UserResult.Failure.ExternalLoginTokenInvalid;
        }
    }
}
