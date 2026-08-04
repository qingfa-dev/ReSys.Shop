using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    /// <summary>Maps POST api/storefront/payment/confirm/{paymentId} to confirm a payment after gateway processing.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/payment/confirm/{paymentId} — confirm payment after gateway
            app.MapPost(PaymentFeature.Storefront.Payment.Confirm.Route, async (
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
            .WithTags(PaymentFeature.Tags.Payment)
            .WithSummary(PaymentFeature.Storefront.Payment.Confirm.Summary)
            .WithDescription(PaymentFeature.Storefront.Payment.Confirm.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}