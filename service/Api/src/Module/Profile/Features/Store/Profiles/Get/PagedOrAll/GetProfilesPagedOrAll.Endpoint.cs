using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Profile.Get.PagedOrAll;

public static partial class GetProfilesPagedOrAll
{
    /// <summary>
    /// Represents the API endpoint for retrieving profiles with pagination or all profiles.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving profiles to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving profiles, supporting pagination and querying.
            app.MapGet(ProfileFeature.Store.Profile.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a query from the incoming parameters.
                var query = new Query(parameters);
                // Send: Dispatch the query to the mediator for processing.
                var result = await sender.Send(query, ct);
                // Map: Convert the result to a PagedResult for the HTTP response.
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .HasPermission(ProfileFeature.Admin.Profiles.GetAll.Permission)
            .WithName(nameof(GetProfilesPagedOrAll))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Store.Profile.GetAll.Summary)
            .WithDescription(ProfileFeature.Store.Profile.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
