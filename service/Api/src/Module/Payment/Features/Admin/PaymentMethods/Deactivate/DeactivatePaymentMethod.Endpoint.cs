using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Deactivate;

public static partial class DeactivatePaymentMethod
{
    /// <summary>Maps PATCH api/payment/payment-methods/{id}/deactivate to deactivate a payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/payment/payment-methods/{id}/deactivate — deactivate payment method
            app.MapPatch(PaymentFeature.Admin.PaymentMethods.Deactivate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeactivatePaymentMethod))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.Deactivate.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.Deactivate.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.Deactivate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}