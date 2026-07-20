namespace Module.Identity.Features.Admin.Roles.Permissions.Get;

public static partial class GetRolePermissions
{
    /// <summary>Maps the role permission retrieval route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/roles/{id}/permissions — get permissions for a role
            app.MapGet(IdentityFeature.Admin.Roles.Permissions.Get.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
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