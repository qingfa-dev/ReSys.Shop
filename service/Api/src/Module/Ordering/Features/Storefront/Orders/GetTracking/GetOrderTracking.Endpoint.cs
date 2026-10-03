using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

public static partial class GetOrderTracking
{
    /// <summary>Maps the storefront order tracking timeline route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/orders/{id:guid}/tracking — retrieve order tracking timestamps
            app.MapGet(OrderingFeature.Storefront.Orders.GetTracking.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetOrderTracking))
            .WithTags(OrderingFeature.Tags.Order)
            .WithSummary(OrderingFeature.Storefront.Orders.GetTracking.Summary)
            .WithDescription(OrderingFeature.Storefront.Orders.GetTracking.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
