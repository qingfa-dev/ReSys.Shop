using BuildingBlocks.Authorization.Attributes;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionRules.Create;

public static partial class CreatePromotionRule
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.PromotionRules.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePromotionRule))
            .WithTags(PromotionsFeature.Tags.PromotionRule)
            .HasPermission(PromotionsFeature.Admin.PromotionRules.Create.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionRules.Create.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionRules.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
