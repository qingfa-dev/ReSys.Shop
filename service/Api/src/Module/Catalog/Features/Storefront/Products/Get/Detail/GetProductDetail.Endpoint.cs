using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

public static partial class GetProductDetail
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.Detail.Route, async (
                [FromRoute] string slug,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(slug);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetProductDetail))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Detail.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Detail.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
