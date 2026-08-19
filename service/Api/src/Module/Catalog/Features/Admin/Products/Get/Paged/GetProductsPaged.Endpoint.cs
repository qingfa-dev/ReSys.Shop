using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Get.Paged;

public static partial class GetProductsPagedList
{
    /// <summary>
    /// GET endpoint that retrieves a paged list of products with filtering
    /// by status, taxon, season, and search term.
    /// Route: api/admin/catalog/products
    /// Permission: Products.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProductsPagedList))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Products.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Products.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}