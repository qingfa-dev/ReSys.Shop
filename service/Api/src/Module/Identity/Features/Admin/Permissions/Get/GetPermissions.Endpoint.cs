using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Permissions.Get;

public static partial class GetPermissions
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
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
