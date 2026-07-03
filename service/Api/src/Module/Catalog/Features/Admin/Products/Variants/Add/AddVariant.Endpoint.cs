using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Add;

public static partial class AddVariant
{
    /// <summary>
    /// POST endpoint that adds a new variant to a product.
    /// Route: api/catalog/products/{productId:guid}/variants
    /// Permission: Products.Variants.Create
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.Add.Route, async (
                [FromRoute] Guid productId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(productId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AddVariant))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Add.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Add.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Add.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
