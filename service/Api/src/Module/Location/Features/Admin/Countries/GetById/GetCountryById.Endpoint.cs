using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.Countries.GetById;

public static partial class GetCountryById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Admin.Countries.GetById.Route, handler: async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Id: id);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCountryById))
            .WithTags(tags: LocationFeature.Admin.Countries.Tags)
            .WithSummary(summary: LocationFeature.Admin.Countries.GetById.Summary)
            .WithDescription(description: LocationFeature.Admin.Countries.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}