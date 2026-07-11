using Microsoft.Extensions.Options;

using Module.Payment.Services.Models;
using Module.Payment.Services.Provider;
using Module.Payment.Services.Provider.Stripe;

namespace Module.UnitTests.Payment.Infrastructure.Gateways.Stripe;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeGatewayAuthorize")]
public class StripeGatewayAuthorizeTests
{
    private readonly StripeSetting _options;
    private readonly StripeGateway _gateway;
    private readonly GatewayOptions _gatewayOptions;

    public StripeGatewayAuthorizeTests()
    {
        _options = new StripeSetting { SecretKey = "sk_test_fake" };
        _gateway = new StripeGateway(Options.Create(_options));

        _gatewayOptions = new GatewayOptions
        {
            Email = "test@example.com",
            StatementDescriptorSuffix = "Test",
            Customer = "test@example.com",
            CustomerId = null,
            Ip = null,
            OrderId = "ORD-AUTH",
            PaymentId = "PAY-AUTH",
            IdempotencyKey = "spree-auth"
        };
    }

    [Fact(DisplayName = "AuthorizeAsync: Should return failure with invalid API key")]
    public async Task AuthorizeAsync_ShouldReturnFailure_WithInvalidKey()
    {
        var result = await _gateway.AuthorizeAsync(50m, "pm_fake", _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "PurchaseAsync: Should return failure with invalid API key")]
    public async Task PurchaseAsync_ShouldReturnFailure_WithInvalidKey()
    {
        var result = await _gateway.PurchaseAsync(50m, "pm_fake", _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "CaptureAsync: With responseCode should attempt capture")]
    public async Task CaptureAsync_WithResponseCode_ShouldAttemptCapture()
    {
        var result = await _gateway.CaptureAsync(50m, "pi_fake_intent", _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "VoidAsync: With responseCode should attempt cancel")]
    public async Task VoidAsync_WithResponseCode_ShouldAttemptCancel()
    {
        var result = await _gateway.VoidAsync("pi_fake_intent", null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "RefundAsync: With invalid key should return failure")]
    public async Task RefundAsync_WithInvalidKey_ShouldReturnFailure()
    {
        var result = await _gateway.RefundAsync(30m, "pi_fake_intent", _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("Stripe.");
    }
}
