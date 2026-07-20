using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.ById;

public static partial class GetPaymentMethodById
{
    /// <summary>Maps GET api/payment/payment-methods/{id} to retrieve a payment method by its ID.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/payment/payment-methods/{id} — get payment method by ID
            app.MapGet(PaymentFeature.Admin.PaymentMethods.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPaymentMethodById))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.GetById.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.GetById.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}