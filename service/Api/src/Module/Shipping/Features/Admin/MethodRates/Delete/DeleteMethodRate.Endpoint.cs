using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.MethodRates.Delete;

public static partial class DeleteMethodRate
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ShippingFeature.Admin.MethodRates.Delete.Route, async (
                [FromRoute] Guid rateId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(rateId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteMethodRate))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .HasPermission(ShippingFeature.Admin.MethodRates.Delete.Permission)
            .WithSummary(ShippingFeature.Admin.MethodRates.Delete.Summary)
            .WithDescription(ShippingFeature.Admin.MethodRates.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
