using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Shared.Admin.Permissions.Get;

public static partial class GetPermissions
{
    /// <summary>Maps the permission metadata listing route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/permissions — list all available permission metadata
            app.MapGet(IdentityFeature.Admin.Permissions.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                PagedResult<PermissionMetadata> result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetPermissions))
            .WithTags(IdentityFeature.Tags.Permission)
            .HasPermission(IdentityFeature.Admin.Permissions.Get.Permission)
            .WithSummary(IdentityFeature.Admin.Permissions.Get.Summary)
            .WithDescription(IdentityFeature.Admin.Permissions.Get.Description)
            .Produces<PagedResult<PermissionMetadata>>()
            .Produces<PagedResult<PermissionMetadata>>(StatusCodes.Status401Unauthorized)
            .Produces<PagedResult<PermissionMetadata>>(StatusCodes.Status404NotFound);
        }
    }
}