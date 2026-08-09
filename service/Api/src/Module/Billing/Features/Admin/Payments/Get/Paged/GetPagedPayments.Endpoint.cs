using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.Payments.Get.Paged;

public static partial class GetPagedPayments
{
    /// <summary>Maps GET api/admin/payment/payments to retrieve a paged list of payments.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/admin/payment/payments — paged list of payments
            app.MapGet(BillingFeature.Admin.Payments.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedPayments))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.Payments.GetAll.Permission)
            .WithSummary(BillingFeature.Admin.Payments.GetAll.Summary)
            .WithDescription(BillingFeature.Admin.Payments.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}