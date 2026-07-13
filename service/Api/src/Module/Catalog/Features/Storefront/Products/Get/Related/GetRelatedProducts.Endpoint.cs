using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Related;

public static partial class GetRelatedProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.Related.Route, async (
                [FromRoute] Guid id,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetRelatedProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Related.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Related.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}