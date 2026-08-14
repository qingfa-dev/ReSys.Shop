using Module.Billing.Services.Provider;
using Module.Billing.Features.Shared;
using Shared.Security.RateLimiting;

namespace Module.Billing.Features.Storefront.Payment.Webhooks;

public static partial class StripeWebhook
{
    private const int MaxWebhookBodySize = 65536; // 64KB — Stripe max payload ~16KB

    /// <summary>Maps POST api/storefront/billing/webhooks/stripe to handle Stripe webhook events.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/billing/webhooks/stripe — handle Stripe webhook events
            app.MapPost(BillingFeature.Storefront.Webhooks.Stripe.Route, async (
                HttpRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Guard: Reject oversized payloads — DoS prevention
                if (request.ContentLength > MaxWebhookBodySize)
                    return Results.StatusCode(StatusCodes.Status413RequestEntityTooLarge);

                using var reader = new StreamReader(request.Body);
                var payload = await reader.ReadToEndAsync(ct);

                // Guard: Reject payloads that exceed limit after reading (Content-Length may be absent)
                if (payload.Length > MaxWebhookBodySize)
                    return Results.StatusCode(StatusCodes.Status413RequestEntityTooLarge);

                var stripeSignature = request.Headers[GatewayConstants.Webhook.Headers.StripeSignature].FirstOrDefault();
                if (string.IsNullOrEmpty(stripeSignature))
                    return Results.BadRequest(GatewayConstants.Webhook.Messages.MissingSignature);

                var command = new Command(payload, stripeSignature);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(StripeWebhook))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.Webhooks.Stripe.Summary)
            .WithDescription(BillingFeature.Storefront.Webhooks.Stripe.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting(RateLimitExtensions.WebhookPolicy);
        }
    }
}