using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.RemoveAdjustment;
public static partial class RemoveOrderAdjustment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(OrderingFeature.Admin.Orders.RemoveAdjustment.Route, async (Guid id, Guid adjustmentId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, adjustmentId), ct);
                return result.ToResult();
            })
            .WithName(nameof(RemoveOrderAdjustment))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.RemoveAdjustment.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.RemoveAdjustment.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.RemoveAdjustment.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
