using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.UpdateShippingMethod;

public static partial class UpdateOrderShippingMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateShippingMethod.Route, async ([FromRoute] Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch UpdateOrderShippingMethod command via MediatR.
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderShippingMethod))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateShippingMethod.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateShippingMethod.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateShippingMethod.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}