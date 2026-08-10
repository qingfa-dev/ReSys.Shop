using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Prices.Remove;

public static partial class RemoveVariantPrice
{
    /// <summary>
    /// DELETE endpoint that removes (soft-deletes) a price for a variant.
    /// Route: api/admin/catalog/variant-prices/{priceId:guid}
    /// Permission: Products.Variants.ManagePrice
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.VariantPrices.Remove.Route, async (
                [FromRoute] Guid priceId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.VariantId, priceId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RemoveVariantPrice))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantPrices.Remove.Permission)
            .WithSummary(CatalogFeature.Admin.VariantPrices.Remove.Summary)
            .WithDescription(CatalogFeature.Admin.VariantPrices.Remove.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}