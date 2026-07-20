using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    /// <summary>Maps POST api/payment/payments/{id}/void to void an authorized payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/payment/payments/{id}/void — gateway void
            app.MapPost(PaymentFeature.Admin.Payments.Void.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(VoidPayment))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.Payments.Void.Permission)
            .WithSummary(PaymentFeature.Admin.Payments.Void.Summary)
            .WithDescription(PaymentFeature.Admin.Payments.Void.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}