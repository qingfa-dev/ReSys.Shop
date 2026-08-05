using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    /// <summary>Maps the product image serving route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/variant-images/{id} — serve product variant image file
            app.MapGet(CatalogFeature.Storefront.VariantImages.Image.Route, async (
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
            .WithSummary(CatalogFeature.Storefront.VariantImages.Image.Summary)
            .WithDescription(CatalogFeature.Storefront.VariantImages.Image.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}