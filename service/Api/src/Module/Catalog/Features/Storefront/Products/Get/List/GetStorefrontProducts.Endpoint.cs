using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class GetStorefrontProductPagedOrAll
{
    /// <summary>Maps the storefront product listing route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products — paged listing with filtering, sorting, and search
            app.MapGet(CatalogFeature.Storefront.Products.List.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStorefrontProductPagedOrAll))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.List.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}