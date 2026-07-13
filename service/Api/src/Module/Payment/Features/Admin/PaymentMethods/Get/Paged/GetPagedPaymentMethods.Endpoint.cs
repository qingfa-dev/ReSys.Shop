// Route: GET api/payment/payment-methods — paged list
using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.Paged;

public static partial class GetPagedPaymentMethods
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PaymentFeature.Admin.PaymentMethods.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedPaymentMethods))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.GetAll.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.GetAll.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}