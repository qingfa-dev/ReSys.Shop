using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.GetById;

public static partial class GetVariantImageById
{
    /// <summary>
    /// GET endpoint that retrieves a single variant image by its identifier.
    /// Route: api/catalog/products/variants/images/{id:guid}
    /// Permission: Products.VariantImageMethod.View
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.Images.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Get-by-id query via MediatR pipeline
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetVariantImageById))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
