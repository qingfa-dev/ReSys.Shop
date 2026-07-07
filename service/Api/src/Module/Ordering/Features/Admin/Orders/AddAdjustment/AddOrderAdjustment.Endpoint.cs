using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.AddAdjustment;

public static partial class AddOrderAdjustment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Admin.Orders.AddAdjustment.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AddOrderAdjustment))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.AddAdjustment.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.AddAdjustment.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.AddAdjustment.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
