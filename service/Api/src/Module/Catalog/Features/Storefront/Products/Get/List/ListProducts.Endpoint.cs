using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.List.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.List.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}