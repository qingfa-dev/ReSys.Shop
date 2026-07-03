using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Collections;

public static partial class GetCollectionPage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.Collections.Route, async (
                [FromRoute] string season,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(season, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetCollectionPage))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Collections.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Collections.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
