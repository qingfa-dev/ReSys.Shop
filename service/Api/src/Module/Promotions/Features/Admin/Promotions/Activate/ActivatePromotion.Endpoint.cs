using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.Promotions.Activate;
public static partial class ActivatePromotion
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.Promotions.Activate.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(ActivatePromotion))
            .WithTags(PromotionsFeature.Tags.Promotion)
            .HasPermission(PromotionsFeature.Admin.Promotions.Activate.Permission)
            .WithSummary(PromotionsFeature.Admin.Promotions.Activate.Summary)
            .WithDescription(PromotionsFeature.Admin.Promotions.Activate.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
