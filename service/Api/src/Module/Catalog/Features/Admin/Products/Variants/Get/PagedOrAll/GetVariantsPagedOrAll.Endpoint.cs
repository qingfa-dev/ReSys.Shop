using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Get.PagedOrAll;

public static partial class GetVariantsPagedOrAll
{
    /// <summary>
    /// GET endpoint that lists all variants for a product.
    /// Route: api/admin/catalog/variants
    /// Permission: Products.Variants.List
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Variants.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetVariantsPagedOrAll))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Variants.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Variants.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Variants.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}