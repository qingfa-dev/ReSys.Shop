using Module.Location.Features.Shared;

namespace Module.Location.Features.Storefront.Countries.GetPagedOrAll;

public static partial class GetStorefrontCountryPagedOrAll
{
    /// <summary>Storefront: list/paginate countries.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route} → paginated/all countries for storefront
            app.MapGet(pattern: LocationFeature.Storefront.Countries.GetAll.Route, handler: async (
                    [AsParameters] Parameters parameters,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new Query(Parameters: parameters);
                    var result = await sender.Send(request: query, cancellationToken: ct);
                    return result.ToPagedResult();
                })
                .WithName(nameof(GetStorefrontCountryPagedOrAll))
                .WithTags(tags: LocationFeature.Storefront.Countries.Tags)
                .WithSummary(summary: LocationFeature.Storefront.Countries.GetAll.Summary)
                .WithDescription(description: LocationFeature.Storefront.Countries.GetAll.Description)
                .Produces<PagedResult<Response>>()
                .Produces<Result>(statusCode: StatusCodes.Status400BadRequest);
        }
    }
}