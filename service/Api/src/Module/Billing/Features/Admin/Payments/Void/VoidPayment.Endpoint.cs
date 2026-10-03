using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    /// <summary>Maps POST api/admin/payment/payments/{id}/void to void an authorized payment.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/payment/payments/{id}/void — gateway void
            app.MapPost(BillingFeature.Admin.Payments.Void.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(VoidPayment))
            .WithTags(BillingFeature.Tags.Payment)
            .HasPermission(BillingFeature.Admin.Payments.Void.Permission)
            .WithSummary(BillingFeature.Admin.Payments.Void.Summary)
            .WithDescription(BillingFeature.Admin.Payments.Void.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}