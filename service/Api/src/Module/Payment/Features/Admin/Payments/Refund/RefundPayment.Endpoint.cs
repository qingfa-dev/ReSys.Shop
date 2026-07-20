using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    /// <summary>Maps POST api/payment/payments/{id}/refund to refund a captured payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/payment/payments/{id}/refund — gateway refund
            app.MapPost(PaymentFeature.Admin.Payments.Refund.Route, async (
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
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.Payments.Refund.Permission)
            .WithSummary(PaymentFeature.Admin.Payments.Refund.Summary)
            .WithDescription(PaymentFeature.Admin.Payments.Refund.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}