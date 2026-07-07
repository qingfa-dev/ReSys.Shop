using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionCategories.Delete;
public static partial class DeletePromotionCategory
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Admin.PromotionCategories.Delete.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePromotionCategory))
            .WithTags(PromotionsFeature.Tags.PromotionCategory)
            .HasPermission(PromotionsFeature.Admin.PromotionCategories.Delete.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionCategories.Delete.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionCategories.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
