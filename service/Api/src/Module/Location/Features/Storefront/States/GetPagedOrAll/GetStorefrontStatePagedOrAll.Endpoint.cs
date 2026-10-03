using Module.Location.Features.Shared;

namespace Module.Location.Features.Storefront.States.GetPagedOrAll;

public static partial class GetStorefrontStatePagedOrAll
{
    /// <summary>Storefront: list/paginate states.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route} → paginated/all states for storefront
            app.MapGet(pattern: LocationFeature.Storefront.States.GetAll.Route, handler: async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Parameters: parameters);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStorefrontStatePagedOrAll))
            .WithTags(tags: LocationFeature.Storefront.States.Tags)
            .WithSummary(summary: LocationFeature.Storefront.States.GetAll.Summary)
            .WithDescription(description: LocationFeature.Storefront.States.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest);
        }
    }
}