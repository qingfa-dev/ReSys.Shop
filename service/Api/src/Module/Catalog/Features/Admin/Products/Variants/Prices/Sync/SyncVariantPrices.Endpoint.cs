using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    /// <summary>
    /// POST endpoint that synchronises the full price list for a variant
    /// (adds, updates, and soft-deletes as needed).
    /// Route: api/admin/catalog/variant-prices/sync
    /// Permission: Products.Variants.ManagePrice
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.VariantPrices.Sync.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.VariantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncVariantPrices))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantPrices.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.VariantPrices.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.VariantPrices.Sync.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}