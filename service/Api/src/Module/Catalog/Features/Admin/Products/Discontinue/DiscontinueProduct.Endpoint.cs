using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Discontinue;

public static partial class DiscontinueProduct
{
    /// <summary>
    /// PATCH endpoint that discontinues (archives) a product by ID.
    /// Route: api/catalog/products/{id:guid}/discontinue
    /// Permission: Products.ManageStatus
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(CatalogFeature.Admin.Products.Discontinue.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DiscontinueProduct))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Discontinue.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Discontinue.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Discontinue.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
