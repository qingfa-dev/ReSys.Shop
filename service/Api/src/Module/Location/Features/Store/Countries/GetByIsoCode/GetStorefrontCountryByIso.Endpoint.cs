using Module.Location.Features.Shared;

namespace Module.Location.Features.Store.Countries.GetByIsoCode;

public static partial class GetStorefrontCountryByIso
{
    /// <summary>Storefront: get country by ISO code.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route}/{isoCode} → country by ISO for storefront
            app.MapGet(pattern: LocationFeature.Storefront.Countries.GetByIso.Route, handler: async (
                [FromRoute] string isoCode,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(IsoCode: isoCode);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStorefrontCountryByIso))
            .WithTags(tags: LocationFeature.Storefront.Countries.Tags)
            .WithSummary(summary: LocationFeature.Storefront.Countries.GetByIso.Summary)
            .WithDescription(description: LocationFeature.Storefront.Countries.GetByIso.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}