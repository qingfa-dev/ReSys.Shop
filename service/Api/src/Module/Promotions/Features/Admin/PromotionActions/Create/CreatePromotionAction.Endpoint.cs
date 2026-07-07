using BuildingBlocks.Authorization.Attributes;

using Carter;
using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.PromotionActions.Create;

public static partial class CreatePromotionAction
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.PromotionActions.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePromotionAction))
            .WithTags(PromotionsFeature.Tags.PromotionAction)
            .HasPermission(PromotionsFeature.Admin.PromotionActions.Create.Permission)
            .WithSummary(PromotionsFeature.Admin.PromotionActions.Create.Summary)
            .WithDescription(PromotionsFeature.Admin.PromotionActions.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
