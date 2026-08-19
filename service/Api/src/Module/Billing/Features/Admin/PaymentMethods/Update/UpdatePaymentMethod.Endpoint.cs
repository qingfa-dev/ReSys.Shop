using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Update;

public static partial class UpdatePaymentMethod
{
    /// <summary>Maps PUT api/admin/payment/payment-methods/{id} to update an existing payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT api/admin/payment/payment-methods/{id} — update payment method
            app.MapPut(BillingFeature.Admin.PaymentMethods.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdatePaymentMethod))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.Update.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.Update.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}