namespace Module.Identity.Features.Admin.Users.GetById;

public static partial class GetUserById
{
    /// <summary>
    /// Represents the API endpoint for retrieving a user by its ID.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving a user by ID to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for users by ID.
            app.MapGet(IdentityFeature.Admin.Users.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a query from the route ID.
                var query = new Query(id);
                // Send: Dispatch the query to the mediator for processing.
                var result = await sender.Send(query, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(GetUserById))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.GetById.Permission)
            .WithSummary(IdentityFeature.Admin.Users.GetById.Summary)
            .WithDescription(IdentityFeature.Admin.Users.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}