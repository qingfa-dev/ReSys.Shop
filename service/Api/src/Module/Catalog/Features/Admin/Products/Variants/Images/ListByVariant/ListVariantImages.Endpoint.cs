using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

public static partial class ListVariantImages
{
    /// <summary>
    /// GET endpoint that lists all images for a variant.
    /// Route: api/catalog/products/{productId:guid}/variants/{variantId:guid}/images
    /// Permission: Products.VariantImageMethod.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.Images.GetAll.Route, async (
                [FromRoute] Guid variantId,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: List-images query via MediatR pipeline
                var query = new Query(variantId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(ListVariantImages))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.GetAll.Description)
            .Produces<Result<Response>>();
        }
    }
}
