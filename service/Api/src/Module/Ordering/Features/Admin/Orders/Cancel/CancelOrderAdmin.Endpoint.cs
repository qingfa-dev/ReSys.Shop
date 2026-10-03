using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
{
    /// <summary>Maps the admin order-cancellation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/ordering/orders/{id:guid}/cancel — admin cancel an order
            app.MapPost(OrderingFeature.Admin.Orders.Cancel.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CancelOrderAdmin))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Cancel.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Cancel.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Cancel.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}