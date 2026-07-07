using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.ListOrders;

public static partial class ListCustomerOrders
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Storefront.Orders.List.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListCustomerOrders))
            .WithTags(OrderingFeature.Tags.Order)
            .WithSummary(OrderingFeature.Storefront.Orders.List.Summary)
            .WithDescription(OrderingFeature.Storefront.Orders.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
