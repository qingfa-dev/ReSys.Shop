using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Options.Assign;

public static partial class AssignProductOptionTypes
{
    /// <summary>
    /// POST endpoint that assigns option types to a product.
    /// Route: api/catalog/product-option-types/assign
    /// Permission: Products.OptionTypes.Assign
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.ProductOptionTypes.Assign.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AssignProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.ProductOptionTypes.Assign.Permission)
            .WithSummary(CatalogFeature.Admin.ProductOptionTypes.Assign.Summary)
            .WithDescription(CatalogFeature.Admin.ProductOptionTypes.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}