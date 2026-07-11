using Microsoft.Extensions.Options;

using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Bogus;

namespace Module.UnitTests.Payment.Infrastructure;

public class BogusGatewayTests
{
    private static BogusGateway CreateGateway() => new(Options.Create(new BogusOptions()));

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
        Assert.Contains("captured", response.Value!.Message, StringComparison.OrdinalIgnoreCase);
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
}
