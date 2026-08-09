using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Delete;

public static partial class DeletePaymentMethod
{
    /// <summary>Maps DELETE api/admin/payment/payment-methods/{id} to soft-delete a payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE api/admin/payment/payment-methods/{id} — soft-delete payment method
            app.MapDelete(BillingFeature.Admin.PaymentMethods.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePaymentMethod))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.Delete.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.Delete.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}