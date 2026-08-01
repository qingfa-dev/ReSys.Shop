using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

public static partial class ListVariantImages
{
    /// <summary>
    /// GET endpoint that lists all images for a variant.
    /// Route: api/catalog/variant-images?variantId=
    /// Permission: Products.VariantImageMethod.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.VariantImages.GetAll.Route, async (
                [FromQuery] Guid variantId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: List-images query via MediatR pipeline
                var query = new Query(variantId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListVariantImages))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.VariantImages.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImages.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImages.GetAll.Description)
            .Produces<PagedResult<VariantImageDetailResponse>>();
        }
    }
}
