using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.GetPagedOrAll;

public static partial class GetStatePagedOrAll
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(pattern: LocationFeature.Admin.States.GetAll.Route, handler: async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Parameters: parameters);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStatePagedOrAll))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.GetAll.Summary)
            .WithDescription(description: LocationFeature.Admin.States.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized);
        }
    }
}