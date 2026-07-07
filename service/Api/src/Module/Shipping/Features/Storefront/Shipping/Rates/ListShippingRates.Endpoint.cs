using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Storefront.Shipping.Rates.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListShippingRates))
            .WithTags(ShippingFeature.Tags.ShippingRate)
            .WithSummary(ShippingFeature.Storefront.Shipping.Rates.Summary)
            .WithDescription(ShippingFeature.Storefront.Shipping.Rates.Description)
            .Produces<PagedResult<ListShippingRates.Response>>();
        }
    }
}
