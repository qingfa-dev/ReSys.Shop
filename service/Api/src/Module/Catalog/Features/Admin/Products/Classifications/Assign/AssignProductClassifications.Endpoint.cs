using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Classifications.Assign;

public static partial class AssignProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Classifications.Assign.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AssignProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Classifications.Assign.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Classifications.Assign.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Classifications.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}