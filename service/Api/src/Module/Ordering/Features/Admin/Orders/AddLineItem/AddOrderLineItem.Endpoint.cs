using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.AddLineItem;
public static partial class AddOrderLineItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Admin.Orders.AddLineItem.Route, async (Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(AddOrderLineItem))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.AddLineItem.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.AddLineItem.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.AddLineItem.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
