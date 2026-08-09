using FluentAssertions;
using Microsoft.Extensions.Options;

using Module.Billing.Services.Provider;
using Module.Billing.Services.Provider.Bogus;

namespace Module.UnitTests.Payment.Infrastructure;

public class BogusGatewayTests
{
    private static BogusGateway CreateGateway() => new(Options.Create(new BogusSetting()));

    private static GatewayOptions CreateGatewayOptions()
    {
        return new GatewayOptions
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

    [Fact]
    public async Task PurchaseAsync_WithSuccessCard_ReturnsSucceeded()
    {
        var gateway = CreateGateway();
        var response = await gateway.PurchaseAsync(
            amount: 1000m,
            source: BogusGateway.TestCards.Success,
            options: CreateGatewayOptions());
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value!.Authorization);
    }

    [Fact]
    public async Task PurchaseAsync_WithDeclinedCard_ReturnsFailure()
    {
        var gateway = CreateGateway();
        var response = await gateway.PurchaseAsync(
            amount: 1000m,
            source: BogusGateway.TestCards.Declined,
            options: CreateGatewayOptions());
        Assert.True(response.IsFailure);
    }

    [Fact]
    public async Task PurchaseAsync_WithInsufficientFundsCard_ReturnsFailure()
    {
        var gateway = CreateGateway();
        var response = await gateway.PurchaseAsync(
            amount: 1000m,
            source: BogusGateway.TestCards.InsufficientFunds,
            options: CreateGatewayOptions());
        Assert.True(response.IsFailure);
    }

    [Fact(DisplayName = "GetPaymentStatusAsync returns correct status after successful purchase")]
    public async Task GetPaymentStatusAsync_ShouldReturnStatus_FromSimulatedIntent()
    {
        var gateway = CreateGateway();
        var response = await gateway.PurchaseAsync(
            amount: 1000m,
            source: BogusGateway.TestCards.Success,
            options: CreateGatewayOptions());
        Assert.True(response.IsSuccess);

        var authorization = response.Value!.Authorization;
        Assert.NotNull(authorization);
        var status = await gateway.GetPaymentStatusAsync(authorization, TestContext.Current.CancellationToken);
        status.Should().Be("succeeded");
    }

    [Fact(DisplayName = "BogusGateway: GetPaymentStatusAsync should return unknown for unrecognized code")]
    public async Task GetPaymentStatusAsync_ShouldReturnUnknown_ForUnknownCode()
    {
        var gateway = new BogusGateway(Options.Create(new BogusSetting { Enabled = true }));

        var result = await gateway.GetPaymentStatusAsync("nonexistent_code");

        result.Should().Be("unknown");
    }
}
