using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Get.AdjustmentById;

public static partial class GetOrderAdjustmentById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(OrderingFeature.Admin.Orders.GetAdjustmentById.Route, async (Guid id, Guid adjustmentId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id, adjustmentId), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderAdjustmentById))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.GetAdjustmentById.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.GetAdjustmentById.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.GetAdjustmentById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
