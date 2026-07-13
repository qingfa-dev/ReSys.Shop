using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Delete;

public static partial class DeleteProduct
{
    /// <summary>
    /// DELETE endpoint that soft-deletes a product by ID, cascading to all variants.
    /// Route: api/catalog/products/{id:guid}
    /// Permission: Products.Delete
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Products.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteProduct))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}