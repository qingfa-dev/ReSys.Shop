using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
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
