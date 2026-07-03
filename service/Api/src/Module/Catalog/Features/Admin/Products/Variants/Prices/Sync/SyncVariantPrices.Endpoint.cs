using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    /// <summary>
    /// POST endpoint that synchronises the full price list for a variant
    /// (adds, updates, and soft-deletes as needed).
    /// Route: api/catalog/products/variants/{variantId:guid}/prices/sync
    /// Permission: Products.Variants.ManagePrice
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.Prices.Sync.Route, async (
                [FromRoute] Guid variantId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(variantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncVariantPrices))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Prices.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Prices.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Prices.Sync.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
