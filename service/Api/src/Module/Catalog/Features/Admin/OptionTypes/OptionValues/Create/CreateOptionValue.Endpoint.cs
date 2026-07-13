using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Create;

public static partial class CreateOptionValue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.OptionTypes.OptionValues.Create.Route, async (
                [FromRoute] Guid optionTypeId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(optionTypeId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateOptionValue))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .HasPermission(CatalogFeature.Admin.OptionTypes.OptionValues.Create.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.OptionValues.Create.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.OptionValues.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}