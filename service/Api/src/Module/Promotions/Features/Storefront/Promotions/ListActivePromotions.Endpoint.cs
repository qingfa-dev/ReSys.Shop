using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Storefront.Promotions;
public static partial class ListActivePromotions
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Storefront.Promotions.ListActive.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListActivePromotions)).WithTags(PromotionsFeature.Tags.Promotion)
            .WithSummary(PromotionsFeature.Storefront.Promotions.ListActive.Summary)
            .WithDescription(PromotionsFeature.Storefront.Promotions.ListActive.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
