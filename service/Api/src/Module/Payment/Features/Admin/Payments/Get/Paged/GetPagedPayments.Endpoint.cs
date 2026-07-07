using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Get.Paged;

public static partial class GetPagedPayments
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PaymentFeature.Admin.Payments.GetAll.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(new QueryingParameters());
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
