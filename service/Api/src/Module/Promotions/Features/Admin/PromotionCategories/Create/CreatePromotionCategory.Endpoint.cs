using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionCategories.Create;
public static partial class CreatePromotionCategory
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.PromotionCategories.Create.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePromotionCategory))
            .WithTags(PromotionsFeature.Tags.PromotionCategory)
            .HasPermission(PromotionsFeature.Admin.PromotionCategories.Create.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionCategories.Create.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionCategories.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
