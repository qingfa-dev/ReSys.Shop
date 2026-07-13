using Module.Identity.Features.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Permissions.Get;

public static partial class GetUserPermissions
{
    public sealed record Response : CategoryGroupListResponse<CategoryResponse, ResourceResponse>;

    public sealed record CategoryResponse : CategoryGroupListItemResponse<ResourceResponse>;

    public sealed record ResourceResponse : ResourceGroupListItemResponse<PermissionItemResponse>;

    public sealed record PermissionItemResponse() : PermissionResponse
    {
        public bool IsAssigned { get; init; }
    }
}
