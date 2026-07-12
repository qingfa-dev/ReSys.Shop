using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

public static partial class GetCustomerOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Storefront.Orders.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                // Call: Dispatch get-customer-order query.
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
