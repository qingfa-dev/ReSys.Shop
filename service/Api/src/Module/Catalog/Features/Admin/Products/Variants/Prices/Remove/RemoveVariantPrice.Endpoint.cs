using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Remove;

public static partial class RemoveVariantPrice
{
    /// <summary>
    /// DELETE endpoint that removes (soft-deletes) a price for a variant.
    /// Route: api/catalog/products/variants/{variantId:guid}/prices/{priceId:guid}
    /// Permission: Products.Variants.ManagePrice
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Products.Variants.Prices.Remove.Route, async (
                [FromRoute] Guid variantId,
                [FromRoute] Guid priceId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(variantId, priceId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RemoveVariantPrice))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Prices.Remove.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Prices.Remove.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Prices.Remove.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
