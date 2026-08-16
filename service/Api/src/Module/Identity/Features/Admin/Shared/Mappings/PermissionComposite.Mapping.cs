using Module.Identity.Features.Admin.Shared.Models;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Shared.Mappings;

public static class PermissionCompositeMapping
{
    public static T MapToPermissionComposite<T, TCategory, TResource, TPermission>(
        this IReadOnlyList<PermissionMetadata> source,
        ISet<string> assignedIdentifiers)
        where T : CategoryGroupListResponse<TCategory, TResource>, new()
        where TCategory : CategoryGroupListItemResponse<TResource>, new()
        where TResource : ResourceGroupListItemResponse<TPermission>, new()
        where TPermission : PermissionAssignmentItemResponse, new()
    {
        var categories = source
            .GroupBy(p => p.Category)
            .Select(categoryGroup => new TCategory
            {
                Category = categoryGroup.Key,
                Resources = [.. categoryGroup
                    .GroupBy(p => p.Resource)
                    .Select(resourceGroup => new TResource
                    {
                        Resource = resourceGroup.Key,
                        Permissions = [.. resourceGroup.Select(permission => new TPermission
                        {
                            Identifier = permission.Identifier,
                            Name = permission.Name,
                            Description = permission.Description,
                            Action = permission.Action,
                            IsAssigned = assignedIdentifiers.Contains(permission.Identifier)
                        })]
                    })]
            })
            .ToList();

        return new T { Categories = categories };
    }
}
