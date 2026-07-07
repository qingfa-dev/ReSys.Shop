using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.Promotions.Update;
public static partial class UpdatePromotion
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(PromotionsFeature.Admin.Promotions.Update.Route, async ([FromRoute] Guid id, [FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdatePromotion))
            .WithTags(PromotionsFeature.Tags.Promotion)
            .HasPermission(PromotionsFeature.Admin.Promotions.Update.Permission)
            .WithSummary(PromotionsFeature.Admin.Promotions.Update.Summary)
            .WithDescription(PromotionsFeature.Admin.Promotions.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
