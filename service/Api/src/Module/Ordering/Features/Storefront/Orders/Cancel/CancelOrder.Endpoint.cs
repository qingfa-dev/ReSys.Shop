using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Orders.Cancel;

public static partial class CancelOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Storefront.Orders.Cancel.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                // Call: Dispatch cancel-order command.
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(CancelOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .WithSummary(OrderingFeature.Storefront.Orders.Cancel.Summary)
            .WithDescription(OrderingFeature.Storefront.Orders.Cancel.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
