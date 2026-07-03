using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

public static partial class ListVariantsByProduct
{
    /// <summary>
    /// GET endpoint that lists all variants for a product.
    /// Route: api/catalog/products/{productId:guid}/variants
    /// Permission: Products.Variants.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.GetAll.Route, async (
                [FromRoute] Guid productId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(productId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(ListVariantsByProduct))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.GetAll.Description)
            .Produces<Result<Response>>();
        }
    }
}
