using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Shipments.Create;
public static partial class CreateOrderShipment
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Admin.Orders.CreateShipment.Route, async (Guid orderId, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(orderId, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateOrderShipment))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.CreateShipment.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.CreateShipment.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.CreateShipment.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
