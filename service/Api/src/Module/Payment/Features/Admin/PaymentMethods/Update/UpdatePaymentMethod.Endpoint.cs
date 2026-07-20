using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Update;

public static partial class UpdatePaymentMethod
{
    /// <summary>Maps PUT api/payment/payment-methods/{id} to update an existing payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT api/payment/payment-methods/{id} — update payment method
            app.MapPut(PaymentFeature.Admin.PaymentMethods.Update.Route, async (
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
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.Update.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.Update.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}