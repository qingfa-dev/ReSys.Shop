using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.Promotions.Delete;
public static partial class DeletePromotion
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PromotionsFeature.Admin.Promotions.Delete.Route, async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePromotion))
            .WithTags(PromotionsFeature.Tags.Promotion)
            .HasPermission(PromotionsFeature.Admin.Promotions.Delete.Permission)
            .WithSummary(PromotionsFeature.Admin.Promotions.Delete.Summary)
            .WithDescription(PromotionsFeature.Admin.Promotions.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
