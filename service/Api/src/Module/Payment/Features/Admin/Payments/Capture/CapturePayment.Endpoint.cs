using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    /// <summary>Maps POST api/payment/payments/{id}/capture to capture an authorized payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/payment/payments/{id}/capture — gateway capture
            app.MapPost(PaymentFeature.Admin.Payments.Capture.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CapturePayment))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.Payments.Capture.Permission)
            .WithSummary(PaymentFeature.Admin.Payments.Capture.Summary)
            .WithDescription(PaymentFeature.Admin.Payments.Capture.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}