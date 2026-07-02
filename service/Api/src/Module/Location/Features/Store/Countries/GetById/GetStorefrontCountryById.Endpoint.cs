using Module.Location.Features.Shared;

namespace Module.Location.Features.Store.Countries.GetById;

public static partial class GetStorefrontCountryById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Storefront.Countries.GetById.Route, handler: async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Id: id);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStorefrontCountryById))
            .WithTags(tags: LocationFeature.Storefront.Countries.Tags)
            .WithSummary(summary: LocationFeature.Storefront.Countries.GetById.Summary)
            .WithDescription(description: LocationFeature.Storefront.Countries.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}