using Module.Location.Features.Shared;

namespace Module.Location.Features.Store.States.GetById;

public static partial class GetStorefrontStateById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Storefront.States.GetById.Route, handler: async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Id: id);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStorefrontStateById))
            .WithTags(tags: LocationFeature.Storefront.States.Tags)
            .WithSummary(summary: LocationFeature.Storefront.States.GetById.Summary)
            .WithDescription(description: LocationFeature.Storefront.States.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}