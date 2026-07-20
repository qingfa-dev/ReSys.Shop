using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.ListOrders;

public static partial class ListCustomerOrders
{
    /// <summary>Maps the storefront customer-orders listing route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/orders — list current user's orders with paging
            app.MapGet(OrderingFeature.Storefront.Orders.List.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ListCustomerOrders))
            .WithTags(OrderingFeature.Tags.Order)
            .WithSummary(OrderingFeature.Storefront.Orders.List.Summary)
            .WithDescription(OrderingFeature.Storefront.Orders.List.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}