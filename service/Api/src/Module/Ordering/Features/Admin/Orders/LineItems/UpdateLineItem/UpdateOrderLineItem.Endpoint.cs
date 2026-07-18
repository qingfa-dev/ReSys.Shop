using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.LineItems.UpdateLineItem;

public static partial class UpdateOrderLineItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateLineItem.Route, async ([FromRoute] Guid id, [FromRoute] Guid lineItemId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch UpdateOrderLineItem command via MediatR.
                var result = await sender.Send(new Command(id, lineItemId, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderLineItem))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateLineItem.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateLineItem.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateLineItem.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}