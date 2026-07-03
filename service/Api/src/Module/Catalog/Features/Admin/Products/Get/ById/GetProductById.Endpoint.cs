using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Get.ById;

public static partial class GetProductById
{
    /// <summary>
    /// GET endpoint that retrieves a single product by ID with full related data.
    /// Route: api/catalog/products/{id:guid}
    /// Permission: Products.View
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetProductById))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.Products.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.Products.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
