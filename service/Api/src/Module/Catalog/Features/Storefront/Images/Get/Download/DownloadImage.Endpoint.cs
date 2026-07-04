using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Images.Get.Download;

public static partial class DownloadImage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Images.Get.Download.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                if (result.IsFailure)
                    return result.ToResult();

                return Results.File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
            })
            .WithName(nameof(DownloadImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Images.Get.Download.Summary)
            .WithDescription(CatalogFeature.Storefront.Images.Get.Download.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
