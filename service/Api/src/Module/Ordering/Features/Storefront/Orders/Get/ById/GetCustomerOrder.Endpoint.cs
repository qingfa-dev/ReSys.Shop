using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

public static partial class GetCustomerOrder
{
    /// <summary>Maps the storefront customer-order retrieval route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/storefront/orders/{id:guid} — retrieve a customer order by ID
            app.MapGet(OrderingFeature.Storefront.Orders.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetCustomerOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .WithSummary(OrderingFeature.Storefront.Orders.GetById.Summary)
            .WithDescription(OrderingFeature.Storefront.Orders.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}