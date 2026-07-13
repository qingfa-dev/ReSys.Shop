using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Get;

public static partial class ListPricesByVariant
{


    /// <summary>
    /// GET endpoint that lists prices for a variant with pagination.
    /// Route: api/catalog/products/variants/{variantId:guid}/prices
    /// Permission: Products.Variants.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.Prices.List.Route, async (
                Guid variantId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(variantId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListPricesByVariant))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Prices.List.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Prices.List.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Prices.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}