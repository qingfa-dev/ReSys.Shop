using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.Paged;

public static partial class GetPagedPaymentMethods
{
    /// <summary>Maps GET api/admin/payment/payment-methods to retrieve a paged list of payment methods.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/admin/payment/payment-methods — paged list of payment methods
            app.MapGet(BillingFeature.Admin.PaymentMethods.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedPaymentMethods))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.GetAll.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.GetAll.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}