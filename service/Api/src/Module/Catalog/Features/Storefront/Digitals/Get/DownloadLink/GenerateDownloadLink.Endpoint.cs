using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Digitals.Get.DownloadLink;

public static partial class GenerateDownloadLink
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Digitals.Get.DownloadLink.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GenerateDownloadLink))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Digitals.Get.DownloadLink.Summary)
            .WithDescription(CatalogFeature.Storefront.Digitals.Get.DownloadLink.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
