using Module.Billing.Services.Provider;
using Module.Billing.Services.Provider.Stripe;

using Stripe.Checkout;

namespace Module.UnitTests.Payment.Services.Provider.Stripe;

[Trait("Category", "Unit")]
[Trait("Module", "Billing")]
[Trait("Feature", "StripeGatewayCheckoutSession")]
public class StripeGatewayCheckoutSessionTests
{
    private static GatewayOptions BuildOptions() => new()
    {
        Email = "test@example.com",
        Customer = "test@example.com",
        OrderId = "order-0001",
        PaymentId = "PAY-20260816-ABC123",
        IdempotencyKey = "shop-PAY-20260816-ABC123",
        Currency = "USD",
    };

    [Fact(DisplayName = "BuildCheckoutSessionOptions: preserves session metadata and line item")]
    public void BuildCheckoutSessionOptions_PreservesSessionMetadataAndLineItem()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.Metadata.Should().ContainKey(GatewayConstants.Metadata.OrderIdKey)
            .WhoseValue.Should().Be("order-0001");
        so.Metadata.Should().ContainKey(GatewayConstants.Metadata.PaymentIdKey)
            .WhoseValue.Should().Be("PAY-20260816-ABC123");

        so.LineItems.Should().NotBeNull();
        var line = so.LineItems!.Single();
        line.Quantity.Should().Be(1);
        line.PriceData.Should().NotBeNull();
        line.PriceData!.Currency.Should().Be("usd");
        line.PriceData.UnitAmount.Should().Be(10000);
        line.PriceData.ProductData.Should().NotBeNull();
        line.PriceData.ProductData!.Name.Should().Be("Order order-0001");
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: propagates metadata to the PaymentIntent")]
    public void BuildCheckoutSessionOptions_PopulatesPaymentIntentMetadata()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.PaymentIntentData.Should().NotBeNull();
        so.PaymentIntentData!.Metadata.Should().ContainKey(GatewayConstants.Metadata.OrderIdKey)
            .WhoseValue.Should().Be("order-0001");
        so.PaymentIntentData.Metadata.Should().ContainKey(GatewayConstants.Metadata.PaymentIdKey)
            .WhoseValue.Should().Be("PAY-20260816-ABC123");
    }
}
