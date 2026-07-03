using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Search;

public static partial class SearchProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.Search.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(SearchProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Search.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Search.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
