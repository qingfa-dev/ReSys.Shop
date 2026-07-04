using Microsoft.AspNetCore.Http;

using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Images.Get.Image.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                if (result.IsFailure)
                    return result.ToResult();

                return TypedResults.PhysicalFile(result.Value.FullPath, result.Value.ContentType);
            })
            .WithName(nameof(GetImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Images.Get.Image.Summary)
            .WithDescription(CatalogFeature.Storefront.Images.Get.Image.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
