using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Approve;

public static partial class ApproveOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Admin.Orders.Approve.Route, async ([FromRoute] Guid id, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch ApproveOrder command via MediatR.
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(ApproveOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Approve.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Approve.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Approve.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
