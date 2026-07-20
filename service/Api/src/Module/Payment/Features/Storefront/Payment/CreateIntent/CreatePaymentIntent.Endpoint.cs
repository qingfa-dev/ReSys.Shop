// Route: POST api/storefront/payment/create-intent — create gateway payment intent
using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PaymentFeature.Storefront.Payment.CreateIntent.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.OrderId, request.PaymentMethodId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
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