using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    /// <summary>Maps POST api/admin/payment/payments/{id}/refund to refund a captured payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/payment/payments/{id}/refund — gateway refund
            app.MapPost(BillingFeature.Admin.Payments.Refund.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RefundPayment))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.Payments.Refund.Permission)
            .WithSummary(BillingFeature.Admin.Payments.Refund.Summary)
            .WithDescription(BillingFeature.Admin.Payments.Refund.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}