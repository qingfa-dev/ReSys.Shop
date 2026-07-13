using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;
public static partial class GetOrderLineItems
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetLineItems.Route, async (
                [FromRoute] Guid id,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                // Call: Dispatch GetOrderLineItems query via MediatR.
                var result = await sender.Send(new Query(id, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOrderLineItems))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetLineItems.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetLineItems.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetLineItems.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
