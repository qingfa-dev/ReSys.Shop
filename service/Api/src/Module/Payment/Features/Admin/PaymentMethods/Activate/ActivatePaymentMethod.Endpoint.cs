// Route: PATCH api/payment/payment-methods/{id}/activate — activate payment method
using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Activate;

public static partial class ActivatePaymentMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(PaymentFeature.Admin.PaymentMethods.Activate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ActivatePaymentMethod))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.Activate.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.Activate.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.Activate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}