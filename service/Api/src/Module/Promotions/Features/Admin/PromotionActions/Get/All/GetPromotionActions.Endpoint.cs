using BuildingBlocks.Authorization.Attributes;
using BuildingBlocks.Querying.Models;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionActions.Get.All;

public static partial class GetPromotionActions
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.PromotionActions.GetAll.Route, async (
                Guid promotionId,
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(promotionId, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPromotionActions))
            .WithTags(PromotionsFeature.Tags.PromotionAction)
            .HasPermission(PromotionsFeature.Admin.PromotionActions.GetAll.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionActions.GetAll.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionActions.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
