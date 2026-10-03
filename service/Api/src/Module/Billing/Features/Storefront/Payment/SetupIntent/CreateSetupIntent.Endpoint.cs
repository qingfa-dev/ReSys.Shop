using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    /// <summary>Maps POST api/storefront/payment/setup-intent to create a Stripe SetupIntent.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/payment/setup-intent — create Stripe SetupIntent
            app.MapPost(BillingFeature.Storefront.PaymentMethods.SetupIntent.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.PaymentMethodId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName(nameof(CreateSetupIntent))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.PaymentMethods.SetupIntent.Summary)
            .WithDescription(BillingFeature.Storefront.PaymentMethods.SetupIntent.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}