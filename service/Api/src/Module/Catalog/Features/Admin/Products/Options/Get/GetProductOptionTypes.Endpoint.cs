using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Options.Get;

public static partial class GetProductOptionTypes
{
    /// <summary>
    /// GET endpoint that retrieves all option types with assignment status for a product.
    /// Route: api/catalog/product-option-types
    /// Permission: Products.OptionTypes.Get
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.ProductOptionTypes.Get.Route, async (
                [FromQuery] Guid productId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(productId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.ProductOptionTypes.Get.Permission)
            .WithSummary(CatalogFeature.Admin.ProductOptionTypes.Get.Summary)
            .WithDescription(CatalogFeature.Admin.ProductOptionTypes.Get.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}