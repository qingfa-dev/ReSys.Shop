using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipAddress;
public static partial class UpdateOrderShipAddress
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateShipAddress.Route, async (Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch UpdateOrderShipAddress command via MediatR.
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderShipAddress))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateShipAddress.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateShipAddress.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateShipAddress.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
