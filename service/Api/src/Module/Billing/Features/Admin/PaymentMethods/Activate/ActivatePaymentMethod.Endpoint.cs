using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Activate;

public static partial class ActivatePaymentMethod
{
    /// <summary>Maps PATCH api/admin/payment/payment-methods/{id}/activate to activate a payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH api/admin/payment/payment-methods/{id}/activate — activate payment method
            app.MapPatch(BillingFeature.Admin.PaymentMethods.Activate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ActivatePaymentMethod))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.Activate.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.Activate.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.Activate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}