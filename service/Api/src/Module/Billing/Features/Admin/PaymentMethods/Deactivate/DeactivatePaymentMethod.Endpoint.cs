using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Deactivate;

public static partial class DeactivatePaymentMethod
{
    /// <summary>Maps PATCH api/admin/payment/payment-methods/{id}/deactivate to deactivate a payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/admin/payment/payment-methods/{id}/deactivate — deactivate payment method
            app.MapPatch(BillingFeature.Admin.PaymentMethods.Deactivate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeactivatePaymentMethod))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.Deactivate.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.Deactivate.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.Deactivate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}