using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Options.Sync;

public static partial class SyncProductOptionTypes
{
    /// <summary>
    /// PUT endpoint that synchronises the full set of option type associations for a product.
    /// Route: api/admin/catalog/product-option-types/sync
    /// Permission: Products.OptionTypes.Sync
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.ProductOptionTypes.Sync.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.ProductId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.ProductOptionTypes.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.ProductOptionTypes.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.ProductOptionTypes.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}