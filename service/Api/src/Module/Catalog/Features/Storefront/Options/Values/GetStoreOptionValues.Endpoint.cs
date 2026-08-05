using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Options.Values;

public static partial class GetStoreOptionValues
{
    /// <summary>Maps the option values listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/option-values — paged listing of product option values (size, colour)
            app.MapGet(CatalogFeature.Storefront.OptionValues.All.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStoreOptionValues))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .WithSummary(CatalogFeature.Storefront.OptionValues.All.Summary)
            .WithDescription(CatalogFeature.Storefront.OptionValues.All.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}