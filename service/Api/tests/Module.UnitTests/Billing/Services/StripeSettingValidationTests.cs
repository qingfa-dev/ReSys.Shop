using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Module.Billing.Services.Provider.Stripe;

namespace Module.UnitTests.Payment.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeSettingValidation")]
public class StripeSettingValidationTests
{
    private readonly StripeSettingValidation _validator;

    public StripeSettingValidationTests()
    {
        _validator = new StripeSettingValidation(new Mock<IHostEnvironment>().Object);
    }

    [Fact(DisplayName = "Validation passes when Stripe is disabled with empty secrets")]
    public void Validate_ShouldPass_WhenDisabled()
    {
        var options = new StripeSetting
        {
            Enabled = false,
            SecretKey = "",
            WebhookSecret = ""
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Validation fails when Enabled=true and SecretKey is empty")]
    public void Validate_ShouldFail_WhenEnabledAndSecretKeyEmpty()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "",
            WebhookSecret = "whsec_test"
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SecretKey");
    }

    [Fact(DisplayName = "Validation fails when Enabled=true and WebhookSecret is empty")]
    public void Validate_ShouldFail_WhenEnabledAndWebhookSecretEmpty()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = ""
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("WebhookSecret");
    }

    [Fact(DisplayName = "Validation passes when Enabled=true and both secrets are set")]
    public void Validate_ShouldPass_WhenEnabledAndSecretsSet()
    {
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = "whsec_test"
        };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    private static StripeSettingValidation CreateValidator(string environmentName)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(environmentName);
        return new StripeSettingValidation(environment.Object);
    }

    [Fact(DisplayName = "Validation skips WebhookSecret requirement in Development")]
    public void Development_SkipsWebhookSecretRequirement()
    {
        var validator = CreateValidator(Environments.Development);
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = ""
        };

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Validation requires WebhookSecret outside Development")]
    public void NonDevelopment_RequiresWebhookSecret()
    {
        var validator = CreateValidator(Environments.Production);
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = ""
        };

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("WebhookSecret");
    }

    [Fact(DisplayName = "Validation passes outside Development when WebhookSecret is set")]
    public void NonDevelopment_WithWebhookSecret_Succeeds()
    {
        var validator = CreateValidator(Environments.Production);
        var options = new StripeSetting
        {
            Enabled = true,
            SecretKey = "sk_test_fake",
            WebhookSecret = "whsec_test"
        };

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }
}
