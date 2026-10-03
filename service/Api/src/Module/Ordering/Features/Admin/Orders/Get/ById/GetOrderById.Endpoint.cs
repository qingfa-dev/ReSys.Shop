using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.ById;

public static partial class GetOrderById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Call: Dispatch GetOrderById query via MediatR.
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderById))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetById.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetById.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}