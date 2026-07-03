namespace Module.Identity.Features.Admin.Roles.Permissions.Sync;

public static partial class SyncRolePermissions
{
    /// <summary>
    /// Represents the API endpoint for synchronizing role permissions.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for synchronizing permissions to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PUT endpoint to sync permissions.
            // Using a specific /sync sub-resource to differentiate from the base PUT if needed, 
            // or we could replace the base PUT. Let's use /sync for absolute clarity.
            app.MapPatch(IdentityFeature.Admin.Roles.Permissions.Sync.Route, async (
                Guid id,
                Request request,
                ISender sender,
                CancellationToken ct) =>/*  */
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Roles.Permissions.Sync.Permission)
            .WithName(nameof(SyncRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Sync.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
