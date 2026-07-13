using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.Countries.GetPagedOrAll;

public static partial class GetCountryPagedOrAll
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(LocationFeature.Admin.Countries.GetAll.Route, async (
                    [AsParameters] Parameters parameters,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new Query(parameters);
                    var result = await sender.Send(query, ct);
                    return result.ToPagedResult();
                })
                .WithName(nameof(GetCountryPagedOrAll))
                .WithTags(LocationFeature.Admin.Countries.Tags)
                .WithSummary(LocationFeature.Admin.Countries.GetAll.Summary)
                .WithDescription("Retrieves a paginated list of countries with optional filtering, sorting, and searching.")
                .Produces<PagedResult<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}