using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.RemoveLineItem;

public static partial class RemoveOrderLineItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(OrderingFeature.Admin.Orders.RemoveLineItem.Route, async ([FromRoute] Guid id, [FromRoute] Guid lineItemId, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch RemoveOrderLineItem command via MediatR.
                var result = await sender.Send(new Command(id, lineItemId), ct);
                return result.ToResult();
            })
            .WithName(nameof(RemoveOrderLineItem))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.RemoveLineItem.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.RemoveLineItem.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.RemoveLineItem.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}