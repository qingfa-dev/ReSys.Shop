using Module.Identity.Features.Admin.Permissions.Shared.Models;

using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Permissions.Shared.Mappings;

public static class PermissionMapping
{
    public static T ToItem<T>(this PermissionMetadata permission)
        where T : PermissionResponse, new()
    {
        return new T
        {
            Identifier = permission.Identifier,
            Name = permission.Name,
            Description = permission.Description,
            Action = permission.Action
        };
    }

    public static TResource ToItem<TResource, TPermission>(this ResourceGroup resource)
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionResponse, new()
    {
        return new TResource
        {
            Resource = resource.ResourceName,
            Description = resource.Description,
            Permissions = resource.Permissions
                .Select(p => p.ToItem<TPermission>())
                .ToList()
        };
    }
    public static List<TResource> MapToListItem<TResource, TPermission>(
           this IEnumerable<ResourceGroup> resources)
           where TResource : ResourceGroupListItemResponse<TPermission>, new()
           where TPermission : PermissionResponse, new()
    {
        return resources
            .Select(r => r.ToItem<TResource, TPermission>())
            .ToList();
    }

    public static TResponse ToListResponse<TResponse, TResource, TPermission>(
        this IEnumerable<ResourceGroup> resources)
        where TResponse : ResourceGroupListResponse<TResource, TPermission>, new()
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionResponse, new()
    {
        return new TResponse
        {
            Resources = resources
                .Select(r => r.ToItem<TResource, TPermission>())
                .ToList()
        };
    }

    #region Categories
    public static TCategory ToItem<TCategory, TResource, TPermission>(this PermissionGroup group)
        where TCategory : CategoryGroupListItemResponse<TResource>, new()
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionResponse, new()
    {
        return new TCategory
        {
            Category = group.Category,
            Description = group.Description,
            Resources = group.Resources
                .Select(r => r.ToItem<TResource, TPermission>())
                .ToList()
        };
    }
    public static List<TCategory> ToList<TCategory, TResource, TPermission>(
        this IEnumerable<PermissionGroup> groups)
        where TCategory : CategoryGroupListItemResponse<TResource>, new()
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionResponse, new()
    {
        return groups
            .Select(g => g.ToItem<TCategory, TResource, TPermission>())
            .ToList();
    }

    public static TResponse ToListResponse<TResponse, TCategory, TResource, TPermission>(
        this IEnumerable<PermissionGroup> groups)
        where TResponse : CategoryGroupListResponse<TCategory, TResource>, new()
        where TCategory : CategoryGroupListItemResponse<TResource>, new()
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionResponse, new()
    {
        return new TResponse
        {
            Categories = groups
                .Select(g => g.ToItem<TCategory, TResource, TPermission>())
                .ToList()
        };
    }
    #endregion
}