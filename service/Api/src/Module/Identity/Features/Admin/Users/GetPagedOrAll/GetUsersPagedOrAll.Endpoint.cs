namespace Module.Identity.Features.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    /// <summary>
    /// Represents the API endpoint for retrieving users with optional pagination or all users.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving users to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving users, supporting pagination and querying.
            app.MapGet(IdentityFeature.Admin.Users.GetAll.Route, async (
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
            .WithName(nameof(GetUsersPagedOrAll))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.GetAll.Permission)
            .WithSummary(IdentityFeature.Admin.Users.GetAll.Summary)
            .WithDescription(IdentityFeature.Admin.Users.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<PagedResult<Response>>(StatusCodes.Status404NotFound);
        }
    }
}