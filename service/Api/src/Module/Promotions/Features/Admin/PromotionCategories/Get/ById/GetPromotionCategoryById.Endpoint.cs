using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionCategories.Get.ById;
public static partial class GetPromotionCategoryById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PromotionsFeature.Admin.PromotionCategories.GetById.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPromotionCategoryById))
            .WithTags(PromotionsFeature.Tags.PromotionCategory)
            .HasPermission(PromotionsFeature.Admin.PromotionCategories.GetById.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionCategories.GetById.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionCategories.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
