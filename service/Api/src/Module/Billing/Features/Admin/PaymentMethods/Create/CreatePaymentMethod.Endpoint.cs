using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    /// <summary>Maps POST api/admin/payment/payment-methods to create a new payment method.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/payment/payment-methods — create payment method
            app.MapPost(BillingFeature.Admin.PaymentMethods.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePaymentMethod))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.PaymentMethods.Create.Permission)
            .WithSummary(BillingFeature.Admin.PaymentMethods.Create.Summary)
            .WithDescription(BillingFeature.Admin.PaymentMethods.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}