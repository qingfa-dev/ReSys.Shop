using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Images.Inferences;

public static partial class GetInferenceModels
{
    /// <summary>Maps the visual search models list route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/visual-search/models — list available embedding models
            app.MapGet(CatalogFeature.Storefront.Products.Images.Inferences.Route, async (
                ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return result.ToResult();
            })
            .WithName(nameof(GetInferenceModels))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Images.Inferences.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Images.Inferences.Description)
            .Produces<Result<Response>>();
        }
    }
}
