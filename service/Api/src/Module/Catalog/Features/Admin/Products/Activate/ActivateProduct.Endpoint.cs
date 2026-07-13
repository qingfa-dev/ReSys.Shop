using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Activate;

public static partial class ActivateProduct
{
    /// <summary>
    /// PATCH endpoint that activates a product by ID.
    /// Route: api/catalog/products/{id:guid}/activate
    /// Permission: Products.ManageStatus
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(CatalogFeature.Admin.Products.Activate.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ActivateProduct))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Activate.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Activate.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Activate.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}