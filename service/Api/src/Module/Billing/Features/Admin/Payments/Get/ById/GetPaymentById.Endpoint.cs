using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.Payments.Get.ById;

public static partial class GetPaymentById
{
    /// <summary>Maps GET api/admin/payment/payments/{id} to retrieve a payment by its ID.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/admin/payment/payments/{id} — get payment by ID
            app.MapGet(BillingFeature.Admin.Payments.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPaymentById))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.Payments.GetById.Permission)
            .WithSummary(BillingFeature.Admin.Payments.GetById.Summary)
            .WithDescription(BillingFeature.Admin.Payments.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}