using Module.Billing.Services.Provider;
using Module.Billing.Services.Provider.Stripe;

using Stripe.Checkout;

namespace Module.UnitTests.Billing.Services.Provider.Stripe;

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

    [Fact(DisplayName = "BuildCheckoutSessionOptions: builds one line per product")]
    public void BuildCheckoutSessionOptions_BuildsPerProductLineItems()
    {
        var options = BuildOptions() with
        {
            LineItems =
            [
                new GatewayLineItem("Classic Tee", 2, 12.50m),
                new GatewayLineItem("Jeans", 1, 50.00m)
            ]
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(75m, options);

        so.LineItems.Should().NotBeNull();
        so.LineItems!.Should().HaveCount(2);
        so.LineItems[0].Quantity.Should().Be(2);
        so.LineItems[0].PriceData!.Currency.Should().Be("usd");
        so.LineItems[0].PriceData.UnitAmount.Should().Be(1250);
        so.LineItems[0].PriceData.ProductData!.Name.Should().Be("Classic Tee");
        so.LineItems[1].Quantity.Should().Be(1);
        so.LineItems[1].PriceData!.UnitAmount.Should().Be(5000);
        so.LineItems[1].PriceData.ProductData!.Name.Should().Be("Jeans");
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: adds a shipping option when Shipping > 0")]
    public void BuildCheckoutSessionOptions_AddsShippingOption_WhenShippingPositive()
    {
        var options = BuildOptions() with
        {
            LineItems = [ new GatewayLineItem("Classic Tee", 1, 25.00m) ],
            Shipping = 12.50m,
            ShippingDisplayName = "Express"
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(37.50m, options);

        so.ShippingOptions.Should().NotBeNull();
        var ship = so.ShippingOptions!.Single();
        ship.ShippingRateData.Should().NotBeNull();
        ship.ShippingRateData!.DisplayName.Should().Be("Express");
        ship.ShippingRateData.FixedAmount!.Amount.Should().Be(1250);
        ship.ShippingRateData.FixedAmount.Currency.Should().Be("usd");
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: omits shipping option when Shipping is zero")]
    public void BuildCheckoutSessionOptions_NoShippingOption_WhenShippingZero()
    {
        var options = BuildOptions() with
        {
            LineItems = [ new GatewayLineItem("Classic Tee", 1, 25.00m) ],
            Shipping = 0m
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(25m, options);

        so.ShippingOptions.Should().BeNull();
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: falls back to aggregate line when no line items")]
    public void BuildCheckoutSessionOptions_FallsBackToAggregate_WhenNoLineItems()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.LineItems.Should().NotBeNull();
        var line = so.LineItems!.Single();
        line.Quantity.Should().Be(1);
        line.PriceData!.UnitAmount.Should().Be(10000);
        line.PriceData.ProductData!.Name.Should().Be("Order order-0001");
    }
}
