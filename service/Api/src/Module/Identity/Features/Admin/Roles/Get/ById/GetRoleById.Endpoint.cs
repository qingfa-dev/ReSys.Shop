namespace Module.Identity.Features.Admin.Roles.Get.ById;

public static partial class GetRoleById
{
    /// <summary>
    /// Represents the API endpoint for retrieving a role by its ID.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving a role by ID to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for roles by ID.
            app.MapGet(IdentityFeature.Admin.Roles.GetById.Route, async (
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
            .WithName(nameof(GetRoleById))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.GetById.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.GetById.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
