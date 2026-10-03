using Module.Location.Features.Shared;

namespace Module.Location.Features.Storefront.Countries.GetById;

public static partial class GetStorefrontCountryById
{
    /// <summary>Storefront: get country by ID.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route}/{id} → country by ID for storefront
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