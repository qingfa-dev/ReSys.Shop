using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Images.Get;

public static partial class GetImageById
{
    /// <summary>Maps the product image serving route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/images/{id} — serve product variant image file
            app.MapGet(CatalogFeature.Storefront.Products.Images.Get.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                if (result.IsFailure)
                    return result.ToResult();

                return Results.File(result.Value.Stream, result.Value.ContentType);
            })
            .WithName(nameof(GetImageById))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Products.Images.Get.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Images.Get.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}