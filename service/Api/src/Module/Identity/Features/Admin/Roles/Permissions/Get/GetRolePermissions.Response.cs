using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Get;

public static partial class GetRolePermissions
{
    public sealed record Response : CategoryGroupListResponse<CategoryResponse, ResourceResponse>;

    public sealed record CategoryResponse : CategoryGroupListItemResponse<ResourceResponse>;

    public sealed record ResourceResponse : ResourceGroupListItemResponse<PermissionItemResponse>;

    public sealed record PermissionItemResponse : PermissionAssignmentItemResponse;
}
