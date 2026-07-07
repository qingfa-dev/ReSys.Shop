using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.Webhooks;

public static partial class StripeWebhook
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PaymentFeature.Storefront.Payment.Webhooks.Stripe.Route, async (
                HttpRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Read: Raw request body for signature validation
                using var reader = new StreamReader(request.Body);
                var payload = await reader.ReadToEndAsync(ct);

                // Get: Stripe-Signature header
                var stripeSignature = request.Headers["Stripe-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(stripeSignature))
                    return Results.BadRequest("Missing Stripe-Signature header.");

                var command = new Command(payload, stripeSignature);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(StripeWebhook))
            .WithTags(PaymentFeature.Tags.Payment)
            .WithSummary(PaymentFeature.Storefront.Payment.Webhooks.Stripe.Summary)
            .WithDescription(PaymentFeature.Storefront.Payment.Webhooks.Stripe.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
