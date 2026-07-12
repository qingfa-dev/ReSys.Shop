using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Delete;
public static partial class DeleteOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(OrderingFeature.Admin.Orders.Delete.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch DeleteOrder command via MediatR.
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Delete.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Delete.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
