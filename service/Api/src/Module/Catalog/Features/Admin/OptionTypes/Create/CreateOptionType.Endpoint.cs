using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.Create;

public static partial class CreateOptionType
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.OptionTypes.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateOptionType))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionTypes.Create.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.Create.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}

