namespace Module.Identity.Features.Admin.Roles.Get.PagedOrAll;

public static partial class GetRolesPagedOrAll
{
    /// <summary>
    /// Represents the API endpoint for retrieving roles with optional pagination or all roles.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving roles to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving roles, supporting pagination and querying.
            app.MapGet(IdentityFeature.Admin.Roles.GetAll.Route, async (
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
            .WithName(nameof(GetRolesPagedOrAll))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.GetAll.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.GetAll.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<PagedResult<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
