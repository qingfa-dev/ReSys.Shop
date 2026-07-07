using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Update.Adjustment;

public static partial class UpdateOrderAdjustment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateAdjustment.Route, async (Guid id, Guid adjustmentId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, adjustmentId, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderAdjustment))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateAdjustment.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateAdjustment.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateAdjustment.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
