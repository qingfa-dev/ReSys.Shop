using BuildingBlocks.Authorization.Attributes;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionRules.Delete;

public static partial class DeletePromotionRule
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Admin.PromotionRules.Delete.Route, async (
                Guid promotionId,
                Guid ruleId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(promotionId, ruleId), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePromotionRule))
            .WithTags(PromotionsFeature.Tags.PromotionRule)
            .HasPermission(PromotionsFeature.Admin.PromotionRules.Delete.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionRules.Delete.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionRules.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
