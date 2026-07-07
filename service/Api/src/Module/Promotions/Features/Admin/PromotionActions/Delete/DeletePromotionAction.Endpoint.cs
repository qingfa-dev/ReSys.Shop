using BuildingBlocks.Authorization.Attributes;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionActions.Delete;

public static partial class DeletePromotionAction
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Admin.PromotionActions.Delete.Route, async (
                Guid promotionId,
                Guid actionId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(promotionId, actionId), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePromotionAction))
            .WithTags(PromotionsFeature.Tags.PromotionAction)
            .HasPermission(PromotionsFeature.Admin.PromotionActions.Delete.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionActions.Delete.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionActions.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
