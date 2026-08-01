using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxons.Get.Products;

public static partial class GetProducts
{
    /// <summary>Maps the taxon products route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons/products?taxonId= — paged products within a taxon
            app.MapGet(CatalogFeature.Storefront.Taxons.Products.Route, async (
                [FromQuery] Guid taxonId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(taxonId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Taxons.Products.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxons.Products.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}