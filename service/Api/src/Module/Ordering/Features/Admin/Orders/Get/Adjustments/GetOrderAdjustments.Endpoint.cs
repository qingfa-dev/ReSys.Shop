using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.Adjustments;
public static partial class GetOrderAdjustments
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetAdjustments.Route, async (
                Guid id,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOrderAdjustments))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetAdjustments.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetAdjustments.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetAdjustments.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
