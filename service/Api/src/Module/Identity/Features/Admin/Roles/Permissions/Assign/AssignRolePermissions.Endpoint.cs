namespace Module.Identity.Features.Admin.Roles.Permissions.Assign;

public static partial class AssignRolePermissions
{
    /// <summary>Maps the role permission assignment route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /api/admin/roles/{id}/permissions/assign — assign permissions to a role
            app.MapPut(IdentityFeature.Admin.Roles.Permissions.Assign.Route, async (
                Guid id,
                Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Roles.Permissions.Assign.Permission)
            .WithName(nameof(AssignRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Assign.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}