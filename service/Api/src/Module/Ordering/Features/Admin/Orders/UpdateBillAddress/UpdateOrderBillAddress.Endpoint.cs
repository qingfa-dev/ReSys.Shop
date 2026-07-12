using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.UpdateBillAddress;
public static partial class UpdateOrderBillAddress
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(OrderingFeature.Admin.Orders.UpdateBillAddress.Route, async (Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch UpdateOrderBillAddress command via MediatR.
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOrderBillAddress))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.UpdateBillAddress.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.UpdateBillAddress.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.UpdateBillAddress.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
