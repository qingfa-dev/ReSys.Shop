using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Options.Types;

public static partial class GetStoreOptionTypes
{
    /// <summary>Maps the option types listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/option-types — paged listing of product option types (size, colour)
            app.MapGet(CatalogFeature.Storefront.OptionTypes.All.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStoreOptionTypes))
            .WithTags(CatalogFeature.Tags.OptionType)
            .WithSummary(CatalogFeature.Storefront.OptionTypes.All.Summary)
            .WithDescription(CatalogFeature.Storefront.OptionTypes.All.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}