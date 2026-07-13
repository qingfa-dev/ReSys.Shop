using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.GetByIsoCode;

public static partial class GetStateByIso
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Admin.States.GetByIso.Route, handler: async (
                [FromRoute] string isoCode,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(IsoCode: isoCode);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStateByIso))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.GetByIso.Summary)
            .WithDescription(description: LocationFeature.Admin.States.GetByIso.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound)
            .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}