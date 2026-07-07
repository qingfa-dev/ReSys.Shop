using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PaymentFeature.Admin.PaymentMethods.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePaymentMethod))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.Create.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.Create.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
