using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.VisualSearchModels;

public static partial class ListVisualSearchModels
{
    /// <summary>Maps the visual search models list route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/visual-search/models — list available embedding models
            app.MapGet(CatalogFeature.Storefront.Products.VisualSearchModels.Route, async (
                ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return result.ToResult();
            })
            .WithName(nameof(ListVisualSearchModels))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.VisualSearchModels.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.VisualSearchModels.Description)
            .Produces<Result<Response>>();
        }
    }
}
