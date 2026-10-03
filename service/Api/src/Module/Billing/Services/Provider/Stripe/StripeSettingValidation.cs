using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Module.Billing.Services.Provider.Stripe;

/// <summary>Validates StripeSetting when enabled: requires non-null SecretKey and WebhookSecret (WebhookSecret skipped in Development).</summary>
public sealed class StripeSettingValidation : IValidateOptions<StripeSetting>
{
    private readonly IHostEnvironment _environment;

    public StripeSettingValidation(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, StripeSetting options)
    {
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        var errors = new List<string>();
        if (string.IsNullOrEmpty(options.SecretKey))
            errors.Add("GatewayProviders:stripe:SecretKey is required when Enabled=true.");
        if (!_environment.IsDevelopment() && string.IsNullOrEmpty(options.WebhookSecret))
            errors.Add("GatewayProviders:stripe:WebhookSecret is required when Enabled=true (skipped in Development for stripe listen).");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
