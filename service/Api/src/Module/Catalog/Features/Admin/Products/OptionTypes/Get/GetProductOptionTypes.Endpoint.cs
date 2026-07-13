using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Get;

public static partial class GetProductOptionTypes
{
    /// <summary>
    /// GET endpoint that retrieves all option types with assignment status for a product.
    /// Route: api/catalog/products/{id:guid}/option-types
    /// Permission: Products.OptionTypes.Get
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.OptionTypes.Get.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.OptionTypes.Get.Permission)
            .WithSummary(CatalogFeature.Admin.Products.OptionTypes.Get.Summary)
            .WithDescription(CatalogFeature.Admin.Products.OptionTypes.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}