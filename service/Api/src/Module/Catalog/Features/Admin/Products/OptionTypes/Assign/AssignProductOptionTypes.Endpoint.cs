using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Assign;

public static partial class AssignProductOptionTypes
{
    /// <summary>
    /// POST endpoint that assigns option types to a product.
    /// Route: api/catalog/products/{id:guid}/option-types/assign
    /// Permission: Products.OptionTypes.Assign
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.OptionTypes.Assign.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AssignProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.OptionTypes.Assign.Permission)
            .WithSummary(CatalogFeature.Admin.Products.OptionTypes.Assign.Summary)
            .WithDescription(CatalogFeature.Admin.Products.OptionTypes.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}