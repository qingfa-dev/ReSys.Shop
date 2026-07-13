using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Update;

public static partial class UpdateVariant
{
    /// <summary>
    /// PUT endpoint that updates a variant by ID.
    /// Route: api/catalog/products/variants/{id:guid}
    /// Permission: Products.Variants.Update
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Variants.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateVariant))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}