using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItemById;

public static partial class GetOrderLineItemById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetLineItemById.Route, async (Guid id, Guid lineItemId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id, lineItemId), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderLineItemById))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetLineItemById.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetLineItemById.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetLineItemById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
