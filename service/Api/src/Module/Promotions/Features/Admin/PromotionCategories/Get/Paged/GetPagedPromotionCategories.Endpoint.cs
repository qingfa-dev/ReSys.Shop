using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionCategories.Get.Paged;
public static partial class GetPagedPromotionCategories
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.PromotionCategories.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedPromotionCategories))
            .WithTags(PromotionsFeature.Tags.PromotionCategory)
            .HasPermission(PromotionsFeature.Admin.PromotionCategories.GetAll.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionCategories.GetAll.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionCategories.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
