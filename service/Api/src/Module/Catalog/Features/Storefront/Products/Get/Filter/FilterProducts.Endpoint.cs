using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Filter;

public static partial class FilterProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.Filter.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(FilterProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Filter.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Filter.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
