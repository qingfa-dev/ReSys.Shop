using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    /// <summary>Maps POST api/storefront/payment/create-intent to create a gateway payment intent.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/storefront/payment/create-intent — create gateway payment intent
            app.MapPost(PaymentFeature.Storefront.Payment.CreateIntent.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.OrderId, request.PaymentMethodId, request.PaymentMethodToken, request.ReturnUrl, request.CardNumber, request.Currency);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName(nameof(CreatePaymentIntent))
            .WithTags(PaymentFeature.Tags.Payment)
            .WithSummary(PaymentFeature.Storefront.Payment.CreateIntent.Summary)
            .WithDescription(PaymentFeature.Storefront.Payment.CreateIntent.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}