using System.Security.Cryptography;
using System.Text;

using Api.Tests.Infrastructure;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.Payment.Webhooks;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Payment;

[Trait("Category", "Integration")]
[Trait("Module", "Payment")]
public sealed class StripeWebhookReplayedTests(ApiFixture fixture) : PaymentIntegrationTestBase(fixture)
{
    private const string WebhookSecret = "whsec_integration_test_secret_32+chars";

    [Fact(DisplayName = "Replayed payment_intent.succeeded webhook does not double-process")]
    public async Task ReplayedWebhook_IsIdempotent_AgainstRealDatabase()
    {
        var orderId = Guid.NewGuid();
        var intentId = "pi_replay_" + Guid.NewGuid().ToString("N")[..8];
        var paymentMethodId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO payment.payment_methods (
                    id, name, provider_key, active, auto_capture, display_on, position,
                    preferences, settings, created_at_utc, is_deleted, webhook_enabled
                  ) VALUES (
                    {0}, 'TestWebhookReplay', 'stripe', true, false, 'Both', 0,
                    '{{}}'::jsonb, '', {1}, false, false
                  )",
                new object[] { paymentMethodId, now });

            await db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ordering.orders (
                    id, number, status, checkout_state, currency,
                    item_total, adjustment_total, shipment_total, total, payment_total, outstanding_balance,
                    item_count, is_deleted, created_at_utc
                  ) VALUES (
                    {0}, {1}, 'Draft', 'Address', 'USD',
                    0, 0, 0, 0, 0, 0,
                    0, false, {2}
                  )",
                new object[] { orderId, $"ORD-{Guid.NewGuid():N}"[..13], now });

            var paymentResult = PaymentCaptureMethod.Create(amount: 50m, paymentMethodId: paymentMethodId, orderId: orderId);
            paymentResult.IsSuccess.Should().BeTrue();
            var payment = paymentResult.Value;
            payment.Process();
            payment.Pend();
            payment.ResponseCode = intentId;

            db.Set<PaymentCapture>().Add(payment);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var payload = $$"""
{
  "id": "evt_test_replay",
  "object": "event",
  "api_version": "2026-06-24.dahlia",
  "type": "payment_intent.succeeded",
  "data": {
    "object": {
      "id": "{{intentId}}",
      "object": "payment_intent"
    }
  }
}
""";
        var signature = ComputeStripeSignature(payload, WebhookSecret);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var job = scope.ServiceProvider.GetRequiredService<Module.Billing.Backgrounds.ProcessStripeWebhookEventJob>();

            var first = await sender.Send(new StripeWebhook.Command(payload, signature), TestContext.Current.CancellationToken);
            first.IsSuccess.Should().BeTrue(string.Join("; ", first.Errors.Select(e => e.Code + ": " + e.Message)));

            await job.ExecuteAsync(payload, TestContext.Current.CancellationToken);

            var second = await sender.Send(new StripeWebhook.Command(payload, signature), TestContext.Current.CancellationToken);
            second.IsSuccess.Should().BeTrue(string.Join("; ", second.Errors.Select(e => e.Code + ": " + e.Message)));

            await job.ExecuteAsync(payload, TestContext.Current.CancellationToken);
        }

        using var verifyScope = Fixture.Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payments = await verifyDb.Set<PaymentCapture>()
            .Where(p => p.OrderId == orderId)
            .ToListAsync(TestContext.Current.CancellationToken);

        payments.Should().HaveCount(1);
        payments[0].State.Should().Be(PaymentRecordState.Completed);
    }

    private static string ComputeStripeSignature(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"t={timestamp},v1={hashHex}";
    }
}
