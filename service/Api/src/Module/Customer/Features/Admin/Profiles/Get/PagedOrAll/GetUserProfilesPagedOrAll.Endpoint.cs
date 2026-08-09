using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Admin.Profiles.Get.PagedOrAll;

public static partial class GetUserProfilesPagedOrAll
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
            app.MapGet(ProfileFeature.Admin.Profiles.GetAll.Route, async (
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
            .WithName(nameof(GetUserProfilesPagedOrAll))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Storefront.Profiles.GetAll.Summary)
            .WithDescription(ProfileFeature.Storefront.Profiles.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}