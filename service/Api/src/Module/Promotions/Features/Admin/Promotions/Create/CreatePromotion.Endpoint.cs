using BuildingBlocks.Authorization.Attributes;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Module.Promotions.Features.Shared;

namespace Module.Promotions.Features.Admin.Promotions.Create;
public static partial class CreatePromotion
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(PromotionsFeature.Admin.Promotions.Create.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreatePromotion))
            .WithTags(PromotionsFeature.Tags.Promotion)
            .HasPermission(PromotionsFeature.Admin.Promotions.Create.Permission)
            .WithSummary(PromotionsFeature.Admin.Promotions.Create.Summary)
            .WithDescription(PromotionsFeature.Admin.Promotions.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
