using Shared.Security.Authorization.Attributes;

namespace Module.Identity.Features.Admin.Users.Permissions.Get;

public static partial class GetUserPermissions
{
    /// <summary>
    /// Represents the API endpoint for retrieving permissions associated with a specific user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for getting user permissions to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving permissions by user ID.
            app.MapGet(IdentityFeature.Admin.Users.Permissions.Get.Route, async (
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
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Users.Permissions.Get.Permission)
            .WithName(nameof(GetUserPermissions))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Permissions.Get.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Permissions.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
