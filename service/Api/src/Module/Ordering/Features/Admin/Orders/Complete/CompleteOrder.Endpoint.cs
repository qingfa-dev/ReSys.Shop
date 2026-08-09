using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Complete;

public static partial class CompleteOrder
{
    /// <summary>Maps the admin order-completion route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/admin/ordering/orders/{id:guid}/complete — mark an order as completed
            app.MapPost(OrderingFeature.Admin.Orders.Complete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(CompleteOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Complete.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Complete.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Complete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}