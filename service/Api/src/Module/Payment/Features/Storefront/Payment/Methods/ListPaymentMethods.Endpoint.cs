using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.Methods;

public static partial class ListPaymentMethods
{
    /// <summary>Maps GET api/storefront/payment/methods to list active payment methods for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/payment/methods — list active payment methods
            app.MapGet(PaymentFeature.Storefront.Payment.Methods.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ListPaymentMethods))
            .WithTags(PaymentFeature.Tags.Payment)
            .WithSummary(PaymentFeature.Storefront.Payment.Methods.Summary)
            .WithDescription(PaymentFeature.Storefront.Payment.Methods.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}