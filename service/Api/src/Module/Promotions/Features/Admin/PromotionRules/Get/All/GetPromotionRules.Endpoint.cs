using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionRules.Get.All;

public static partial class GetPromotionRules
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.PromotionRules.GetAll.Route, async (
                Guid promotionId,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(promotionId, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPromotionRules))
            .WithTags(PromotionsFeature.Tags.PromotionRule)
            .HasPermission(PromotionsFeature.Admin.PromotionRules.GetAll.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionRules.GetAll.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionRules.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
