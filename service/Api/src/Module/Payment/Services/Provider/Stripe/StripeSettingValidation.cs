using Microsoft.Extensions.Options;

namespace Module.Payment.Services.Provider.Stripe;

/// <summary>Validates StripeSetting when enabled: requires non-null SecretKey and WebhookSecret.</summary>
public sealed class StripeSettingValidation : IValidateOptions<StripeSetting>
{
    public ValidateOptionsResult Validate(string? name, StripeSetting options)
    {
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        var errors = new List<string>();
        if (string.IsNullOrEmpty(options.SecretKey))
            errors.Add("GatewayProviders:stripe:SecretKey is required when Enabled=true.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
