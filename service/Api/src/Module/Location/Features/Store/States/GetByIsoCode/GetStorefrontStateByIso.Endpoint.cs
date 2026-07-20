using Module.Location.Features.Shared;

namespace Module.Location.Features.Store.States.GetByIsoCode;

public static partial class GetStorefrontStateByIso
{
    /// <summary>Storefront: get state by ISO code.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route}/{isoCode} → state by ISO for storefront
            app.MapGet(pattern: LocationFeature.Storefront.States.GetByIso.Route, handler: async (
                [FromRoute] string isoCode,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(IsoCode: isoCode);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStorefrontStateByIso))
            .WithTags(tags: LocationFeature.Storefront.States.Tags)
            .WithSummary(summary: LocationFeature.Storefront.States.GetByIso.Summary)
            .WithDescription(description: LocationFeature.Storefront.States.GetByIso.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}