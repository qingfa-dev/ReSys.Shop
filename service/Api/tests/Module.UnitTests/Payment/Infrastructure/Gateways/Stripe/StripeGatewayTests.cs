using Microsoft.Extensions.Options;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Module.Payment.Infrastructure.Gateways.Stripe;

namespace Module.UnitTests.Payment.Infrastructure.Gateways.Stripe;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeGateway")]
public class StripeGatewayTests
{
    private readonly StripeOptions _options;
    private readonly StripeGateway _gateway;
    private readonly GatewayOptions _gatewayOptions;

    public StripeGatewayTests()
    {
        _options = new StripeOptions { SecretKey = "sk_test_fake" };
        _gateway = new StripeGateway(Options.Create(_options));

        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _gatewayOptions = new GatewayOptions(payment)
        {
            Email = "test@example.com",
            StatementDescriptorSuffix = "Test",
            Customer = "test@example.com",
            CustomerId = null,
            Ip = null,
            OrderId = "ORD-TEST",
            PaymentId = "PAY-TEST",
            IdempotencyKey = "spree-test"
        };
    }

    [Fact(DisplayName = "StripeGateway: AutoCapture should be true")]
    public void AutoCapture_ShouldBeTrue()
    {
        _gateway.AutoCapture.Should().BeTrue();
    }

    [Fact(DisplayName = "StripeGateway: SourceRequired should be true")]
    public void SourceRequired_ShouldBeTrue()
    {
        _gateway.SourceRequired.Should().BeTrue();
    }

    [Fact(DisplayName = "StripeGateway: PaymentProfilesSupported should be true")]
    public void PaymentProfilesSupported_ShouldBeTrue()
    {
        _gateway.PaymentProfilesSupported.Should().BeTrue();
    }

    [Fact(DisplayName = "StripeGateway: Supports should return true for string source")]
    public void Supports_ShouldReturnTrue_ForString()
    {
        _gateway.Supports("pm_card_visa").Should().BeTrue();
    }

    [Fact(DisplayName = "StripeGateway: Supports should return true for null source")]
    public void Supports_ShouldReturnTrue_ForNull()
    {
        _gateway.Supports(null).Should().BeTrue();
    }

    [Fact(DisplayName = "StripeGateway: Supports should return false for integer source")]
    public void Supports_ShouldReturnFalse_ForInteger()
    {
        _gateway.Supports(42).Should().BeFalse();
    }

    [Fact(DisplayName = "StripeGateway: Constructor should set ApiKey")]
    public void Constructor_ShouldSetApiKey()
    {
        global::Stripe.StripeConfiguration.ApiKey.Should().Be("sk_test_fake");
    }

    [Fact(DisplayName = "StripeGateway: PurchaseAsync with invalid key should return failure")]
    public async Task PurchaseAsync_ShouldReturnFailure_WithInvalidApiKey()
    {
        var result = await _gateway.PurchaseAsync(100m, null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "StripeGateway: AuthorizeAsync with invalid key should return failure")]
    public async Task AuthorizeAsync_ShouldReturnFailure_WithInvalidApiKey()
    {
        var result = await _gateway.AuthorizeAsync(100m, null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Contain("Stripe.");
    }

    [Fact(DisplayName = "StripeGateway: CaptureAsync without responseCode should return failure")]
    public async Task CaptureAsync_ShouldReturnFailure_WithoutResponseCode()
    {
        var result = await _gateway.CaptureAsync(50m, null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Stripe.Capture.MissingIntent");
    }

    [Fact(DisplayName = "StripeGateway: VoidAsync without responseCode should return failure")]
    public async Task VoidAsync_ShouldReturnFailure_WithoutResponseCode()
    {
        var result = await _gateway.VoidAsync(null, null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Stripe.Cancel.MissingIntent");
    }

    [Fact(DisplayName = "StripeGateway: CancelAsync without responseCode should return failure")]
    public async Task CancelAsync_ShouldReturnFailure_WithoutResponseCode()
    {
        var result = await _gateway.CancelAsync(null, null, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Stripe.Cancel.MissingIntent");
    }

    [Fact(DisplayName = "StripeGateway: CreditAsync without responseCode should return failure")]
    public async Task CreditAsync_ShouldReturnFailure_WithoutResponseCode()
    {
        var result = await _gateway.CreditAsync(50m, null, _gatewayOptions, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("Stripe.Credit.MissingIntent");
    }
}
