namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Sync;

public static partial class SyncRolePermissions
{
    /// <summary>Maps the role permission synchronisation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH /api/admin/roles/{id}/permissions/sync — sync all permission assignments for a role
            app.MapPatch(IdentityFeature.Admin.Roles.Permissions.Sync.Route, async (
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