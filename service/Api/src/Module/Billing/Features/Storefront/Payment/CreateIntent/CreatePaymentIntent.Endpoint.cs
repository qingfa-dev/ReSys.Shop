using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    /// <summary>Maps POST api/storefront/payment/create-intent to create a gateway payment intent.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/payment/create-intent — create gateway payment intent
            app.MapPost(BillingFeature.Storefront.Payments.CreateIntent.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName(nameof(CreatePaymentIntent))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.Payments.CreateIntent.Summary)
            .WithDescription(BillingFeature.Storefront.Payments.CreateIntent.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}