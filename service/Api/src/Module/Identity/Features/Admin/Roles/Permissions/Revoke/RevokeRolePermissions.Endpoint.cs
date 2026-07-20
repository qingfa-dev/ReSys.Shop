namespace Module.Identity.Features.Admin.Roles.Permissions.Revoke;

public static partial class RevokeRolePermissions
{
    /// <summary>Maps the role permission revocation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE /api/admin/roles/{id}/permissions/revoke — revoke permissions from a role
            app.MapDelete(IdentityFeature.Admin.Roles.Permissions.Revoke.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Roles.Permissions.Revoke.Permission)
            .WithName(nameof(RevokeRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Revoke.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}