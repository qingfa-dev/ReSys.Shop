using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

public static partial class UpdateOrderShipmentState
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateShipmentState.Route, async ([FromRoute] Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderShipmentState))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateShipmentState.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateShipmentState.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateShipmentState.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
