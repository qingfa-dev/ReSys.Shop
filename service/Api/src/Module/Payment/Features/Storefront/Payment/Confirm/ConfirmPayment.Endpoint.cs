using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PaymentFeature.Storefront.Payment.Confirm.Route, async (
                [FromRoute] Guid paymentId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(paymentId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
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
