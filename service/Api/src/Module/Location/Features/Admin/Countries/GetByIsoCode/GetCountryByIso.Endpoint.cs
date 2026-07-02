using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.Countries.GetByIsoCode;

public static partial class GetCountryByIso
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Admin.Countries.GetByIso.Route, handler: async (
                [FromRoute] string isoCode,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(IsoCode: isoCode);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCountryByIso))
            .WithTags(tags: LocationFeature.Admin.Countries.Tags)
            .WithSummary(summary: LocationFeature.Admin.Countries.GetByIso.Summary)
            .WithDescription(description: LocationFeature.Admin.Countries.GetByIso.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound)
            .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}