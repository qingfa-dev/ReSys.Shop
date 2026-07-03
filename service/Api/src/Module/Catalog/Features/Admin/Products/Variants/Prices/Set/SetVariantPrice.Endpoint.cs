using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Set;

public static partial class SetVariantPrice
{
    /// <summary>
    /// POST endpoint that sets (upserts) a price for a variant by currency and country ISO.
    /// Route: api/catalog/products/variants/{variantId:guid}/prices
    /// Permission: Products.Variants.ManagePrice
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.Prices.Set.Route, async (
                [FromRoute] Guid variantId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(variantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SetVariantPrice))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Prices.Set.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Prices.Set.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Prices.Set.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
