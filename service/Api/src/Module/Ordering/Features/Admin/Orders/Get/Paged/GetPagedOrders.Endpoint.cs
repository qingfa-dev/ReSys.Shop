using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

public static partial class GetPagedOrders
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                // Call: Dispatch GetPagedOrders query via MediatR.
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedOrders))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetAll.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetAll.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
