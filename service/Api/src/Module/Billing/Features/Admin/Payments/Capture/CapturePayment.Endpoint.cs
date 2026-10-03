using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    /// <summary>Maps POST api/admin/payment/payments/{id}/capture to capture an authorized payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/payment/payments/{id}/capture — gateway capture
            app.MapPost(BillingFeature.Admin.Payments.Capture.Route, async (
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
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.Payments.Capture.Permission)
            .WithSummary(BillingFeature.Admin.Payments.Capture.Summary)
            .WithDescription(BillingFeature.Admin.Payments.Capture.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}