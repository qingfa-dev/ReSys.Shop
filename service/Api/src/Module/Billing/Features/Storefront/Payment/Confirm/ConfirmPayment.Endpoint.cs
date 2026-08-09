using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    /// <summary>Maps POST api/storefront/payment/confirm/{paymentId} to confirm a payment after gateway processing.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/payment/confirm/{paymentId} — confirm payment after gateway
            app.MapPost(BillingFeature.Storefront.Payments.Confirm.Route, async (
                [FromRoute] Guid paymentId,
                [FromBody] ConfirmPaymentRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(paymentId, request.PaymentMethodId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName(nameof(ConfirmPayment))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.Payments.Confirm.Summary)
            .WithDescription(BillingFeature.Storefront.Payments.Confirm.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}