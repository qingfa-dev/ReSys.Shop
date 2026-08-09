using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Get;

public static partial class ListPricesByVariant
{


    /// <summary>
    /// GET endpoint that lists prices for a variant with pagination.
    /// Route: api/admin/catalog/variant-prices
    /// Permission: Products.Variants.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.VariantPrices.List.Route, async (
                [FromQuery] Guid variantId,
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
            .HasPermission(CatalogFeature.Admin.VariantPrices.List.Permission)
            .WithSummary(CatalogFeature.Admin.VariantPrices.List.Summary)
            .WithDescription(CatalogFeature.Admin.VariantPrices.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}