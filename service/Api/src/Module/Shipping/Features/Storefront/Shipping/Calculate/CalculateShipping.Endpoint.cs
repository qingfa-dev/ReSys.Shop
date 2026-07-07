using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ShippingFeature.Storefront.Shipping.Calculate.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CalculateShipping))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .WithSummary(ShippingFeature.Storefront.Shipping.Calculate.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Calculate.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
