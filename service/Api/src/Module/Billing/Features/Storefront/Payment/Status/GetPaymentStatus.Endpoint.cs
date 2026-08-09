using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.Status;

public static partial class GetPaymentStatus
{
    /// <summary>Maps GET api/storefront/payment/status/{orderId} to the payment status query.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/payment/status/{orderId} — poll payment state for an order
            app.MapGet(BillingFeature.Storefront.Payments.Status.Route, async (
                [FromRoute] Guid orderId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(orderId), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetPaymentStatus))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.Payments.Status.Summary)
            .WithDescription(BillingFeature.Storefront.Payments.Status.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}