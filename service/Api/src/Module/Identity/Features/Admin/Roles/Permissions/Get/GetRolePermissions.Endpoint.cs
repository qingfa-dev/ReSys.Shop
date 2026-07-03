namespace Module.Identity.Features.Admin.Roles.Permissions.Get;

public static partial class GetRolePermissions
{
    /// <summary>
    /// Represents the API endpoint for retrieving permissions associated with a specific role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for getting role permissions to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving permissions by role ID.
            app.MapGet(IdentityFeature.Admin.Roles.Permissions.Get.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Roles.Permissions.Get.Permission)
            .WithName(nameof(GetRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Get.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
