using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.ById;

public static partial class GetPaymentMethodById
{
    /// <summary>Maps GET api/admin/payment/payment-methods/{id} to retrieve a payment method by its ID.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/admin/payment/payment-methods/{id} — get payment method by ID
            app.MapGet(BillingFeature.Admin.PaymentMethods.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPaymentMethodById))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.GetById.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.GetById.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}