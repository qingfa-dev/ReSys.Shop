using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxonomies.Get.Tree;

public static partial class GetTree
{
    /// <summary>Maps the taxonomy tree route for the storefront navigation.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxonomies/{id}/tree — hierarchical taxonomy tree for navigation
            app.MapGet(CatalogFeature.Storefront.Taxonomies.Get.Tree.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetTree))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .WithSummary(CatalogFeature.Storefront.Taxonomies.Get.Tree.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxonomies.Get.Tree.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}