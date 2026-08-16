using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.Methods;

public static partial class GetPaymentMethods
{
    /// <summary>Maps GET api/storefront/payment/methods to list active payment methods for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/payment/methods — list active payment methods
            app.MapGet(BillingFeature.Storefront.PaymentMethods.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetPaymentMethods))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.PaymentMethods.GetAll.Summary)
            .WithDescription(BillingFeature.Storefront.PaymentMethods.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}