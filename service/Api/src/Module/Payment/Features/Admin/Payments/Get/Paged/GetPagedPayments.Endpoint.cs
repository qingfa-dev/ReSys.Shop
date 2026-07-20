using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Get.Paged;

public static partial class GetPagedPayments
{
    /// <summary>Maps GET api/payment/payments to retrieve a paged list of payments.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/payment/payments — paged list of payments
            app.MapGet(PaymentFeature.Admin.Payments.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedPayments))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.Payments.GetAll.Permission)
            .WithSummary(PaymentFeature.Admin.Payments.GetAll.Summary)
            .WithDescription(PaymentFeature.Admin.Payments.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}