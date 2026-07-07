using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionCategories.Update;
public static partial class UpdatePromotionCategory
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(PromotionsFeature.Admin.PromotionCategories.Update.Route, async ([FromRoute] Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdatePromotionCategory))
            .WithTags(PromotionsFeature.Tags.PromotionCategory)
            .HasPermission(PromotionsFeature.Admin.PromotionCategories.Update.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionCategories.Update.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionCategories.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
