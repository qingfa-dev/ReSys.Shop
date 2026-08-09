using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Storefront.Products.Images.Search;

public static partial class SearchByImage
{
    /// <summary>Maps the image-based product search route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/products/images/search — visual similarity search via embedding
            app.MapPost(CatalogFeature.Storefront.Products.Images.Search.Route, async (
                [FromForm] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(SearchByImage))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Images.Search.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Images.Search.Description)
            .DisableAntiforgery()
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}