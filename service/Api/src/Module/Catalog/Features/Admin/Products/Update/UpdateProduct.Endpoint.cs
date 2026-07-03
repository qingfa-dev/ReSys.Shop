using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Update;

public static partial class UpdateProduct
{
    /// <summary>
    /// PUT endpoint that updates a product by ID, including its master variant.
    /// Route: api/catalog/products/{id:guid}
    /// Permission: Products.Update
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateProduct))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
