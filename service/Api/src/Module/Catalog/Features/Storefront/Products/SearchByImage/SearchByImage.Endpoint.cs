using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Storefront.Products.Get.SearchByImage.Route, async (
                [FromForm] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SearchByImage))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.SearchByImage.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.SearchByImage.Description)
            .DisableAntiforgery()
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}