using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Update;

public static partial class UpdateVariantImage
{
    /// <summary>
    /// PUT endpoint that updates a variant image's metadata (alt, position, type).
    /// Route: api/catalog/products/variants/images/{id:guid}
    /// Permission: Products.VariantImageMethod.Update
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Variants.Images.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Update command via MediatR pipeline
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateVariantImage))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
