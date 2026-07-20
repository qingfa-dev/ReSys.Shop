using Module.Identity.Features.Admin.Permissions.Shared.Mappings;
using Module.Identity.Features.Admin.Permissions.Shared.Models;

using Shared.Security.Identity.Domain.Permissions;

namespace Module.UnitTests.Identity.Features.Admin.Permissions.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Permissions/Mapping")]
public class PermissionMappingTests
{
    private sealed record TestPermissionResponse : PermissionResponse;
    private sealed record TestResourceGroupItem : ResourceGroupListItemResponse<TestPermissionResponse>;
    private sealed record TestResourceGroupList : ResourceGroupListResponse<TestResourceGroupItem, TestPermissionResponse> { }
    private sealed record TestCategoryGroupItem : CategoryGroupListItemResponse<TestResourceGroupItem>;
    private sealed record TestCategoryGroupList : CategoryGroupListResponse<TestCategoryGroupItem, TestResourceGroupItem>;

    [Fact(DisplayName = "Should map single PermissionMetadata to PermissionResponse with all properties")]
    public void ToItem_SinglePermission_ShouldMapAllProperties()
    {
        var permission = PermissionMetadataMethod.For("admin", "identity", "users", "create");

        var result = permission.ToItem<TestPermissionResponse>();

        result.Identifier.Should().Be("admin.identity.users.create");
        result.Name.Should().Be("create users");
        result.Description.Should().Be("Allows create on users in admin/identity.");
        result.Action.Should().Be("create");
    }

    [Fact(DisplayName = "Should map single ResourceGroup with one permission preserving all properties")]
    public void ToItem_SingleResourceGroupWithOnePermission_ShouldMapAllProperties()
    {
        var permission = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var group = new ResourceGroup { ResourceName = "Users", Permissions = [permission], Description = "User management" };

        var result = group.ToItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Resource.Should().Be("Users");
        result.Description.Should().Be("User management");
        result.Permissions.Should().HaveCount(1);
        result.Permissions[0].Identifier.Should().Be("admin.identity.users.create");
    }

    [Fact(DisplayName = "Should map ResourceGroup with multiple permissions preserving all items")]
    public void ToItem_SingleResourceGroupWithMultiplePermissions_ShouldMapAllPermissions()
    {
        var p1 = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var p2 = PermissionMetadataMethod.For("admin", "identity", "users", "view");
        var p3 = PermissionMetadataMethod.For("admin", "identity", "users", "delete");
        var group = new ResourceGroup { ResourceName = "Users", Permissions = [p1, p2, p3] };

        var result = group.ToItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Permissions.Should().HaveCount(3);
        result.Permissions[0].Identifier.Should().Be("admin.identity.users.create");
        result.Permissions[1].Identifier.Should().Be("admin.identity.users.view");
        result.Permissions[2].Identifier.Should().Be("admin.identity.users.delete");
    }

    [Fact(DisplayName = "Should map ResourceGroup with null description preserving null")]
    public void ToItem_ResourceGroupWithNullDescription_ShouldMapNullDescription()
    {
        var permission = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var group = new ResourceGroup { ResourceName = "Users", Permissions = [permission] };

        var result = group.ToItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Description.Should().BeNull();
    }

    [Fact(DisplayName = "Should map ResourceGroup with empty permissions to empty list")]
    public void ToItem_ResourceGroupWithEmptyPermissions_ShouldMapEmptyList()
    {
        var group = new ResourceGroup { ResourceName = "Users", Permissions = [] };

        var result = group.ToItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should map multiple ResourceGroups to list preserving all items")]
    public void ToListItem_MultipleResourceGroups_ShouldMapAllGroups()
    {
        var p1 = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var p2 = PermissionMetadataMethod.For("admin", "identity", "roles", "create");
        var groups = new[]
        {
            new ResourceGroup { ResourceName = "Users", Permissions = [p1], Description = "User management" },
            new ResourceGroup { ResourceName = "Roles", Permissions = [p2], Description = "Role management" }
        };

        var result = groups.MapToListItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Should().HaveCount(2);
        result[0].Resource.Should().Be("Users");
        result[0].Permissions.Should().HaveCount(1);
        result[1].Resource.Should().Be("Roles");
    }

    [Fact(DisplayName = "Should return empty list when ResourceGroup collection is empty")]
    public void ToListItem_EmptyCollection_ShouldReturnEmptyList()
    {
        var groups = Enumerable.Empty<ResourceGroup>();

        var result = groups.MapToListItem<TestResourceGroupItem, TestPermissionResponse>();

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should map multiple ResourceGroups to list response preserving all items")]
    public void ToListResponse_MultipleResourceGroups_ShouldMapResponse()
    {
        var p1 = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var p2 = PermissionMetadataMethod.For("admin", "identity", "roles", "create");
        var groups = new[]
        {
            new ResourceGroup { ResourceName = "Users", Permissions = [p1] },
            new ResourceGroup { ResourceName = "Roles", Permissions = [p2] }
        };

        var result = groups.ToListResponse<TestResourceGroupList, TestResourceGroupItem, TestPermissionResponse>();

        result.Resources.Should().HaveCount(2);
        result.Resources[0].Resource.Should().Be("Users");
        result.Resources[1].Resource.Should().Be("Roles");
    }

    [Fact(DisplayName = "Should return empty resources list when ResourceGroup collection is empty")]
    public void ToListResponse_EmptyCollection_ShouldReturnEmptyResources()
    {
        var groups = Enumerable.Empty<ResourceGroup>();

        var result = groups.ToListResponse<TestResourceGroupList, TestResourceGroupItem, TestPermissionResponse>();

        result.Resources.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should map single PermissionGroup with one resource preserving all properties")]
    public void ToItem_SingleCategoryWithOneResource_ShouldMapAllProperties()
    {
        var permission = PermissionMetadataMethod.For("admin", "identity", "users", "create");
        var resource = new ResourceGroup
        {
            ResourceName = "Users",
            Permissions = [permission],
            Description = "User management"
        };
        var group = new PermissionGroup
        {
            Category = "Identity Management",
            Resources = [resource],
            Description = "Identity related permissions"
        };

        var result = group.ToItem<TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Category.Should().Be("Identity Management");
        result.Description.Should().Be("Identity related permissions");
        result.Resources.Should().HaveCount(1);
        result.Resources[0].Resource.Should().Be("Users");
        result.Resources[0].Permissions[0].Identifier.Should().Be("admin.identity.users.create");
    }

    [Fact(DisplayName = "Should map PermissionGroup with multiple resources preserving all items")]
    public void ToItem_SingleCategoryWithMultipleResources_ShouldMapAllResources()
    {
        var r1 = new ResourceGroup { ResourceName = "Users", Permissions = [PermissionMetadataMethod.For("admin", "identity", "users", "create")] };
        var r2 = new ResourceGroup { ResourceName = "Roles", Permissions = [PermissionMetadataMethod.For("admin", "identity", "roles", "create")] };
        var group = new PermissionGroup { Category = "Identity", Resources = [r1, r2] };

        var result = group.ToItem<TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Resources.Should().HaveCount(2);
        result.Resources[0].Resource.Should().Be("Users");
        result.Resources[1].Resource.Should().Be("Roles");
    }

    [Fact(DisplayName = "Should map PermissionGroup with null description preserving null")]
    public void ToItem_SingleCategoryWithNullDescription_ShouldMapNullDescription()
    {
        var group = new PermissionGroup { Category = "Identity", Resources = [new ResourceGroup { ResourceName = "Users", Permissions = [PermissionMetadataMethod.For("admin", "identity", "users", "create")] }] };

        var result = group.ToItem<TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Description.Should().BeNull();
    }

    [Fact(DisplayName = "Should map multiple PermissionGroups to list preserving all items")]
    public void ToList_MultipleCategories_ShouldMapAllCategories()
    {
        var groups = new[]
        {
            new PermissionGroup { Category = "Identity", Resources = [new ResourceGroup { ResourceName = "Users", Permissions = [PermissionMetadataMethod.For("admin", "identity", "users", "create")] }] },
            new PermissionGroup { Category = "Catalog", Resources = [new ResourceGroup { ResourceName = "Products", Permissions = [PermissionMetadataMethod.For("admin", "catalog", "products", "view")] }] }
        };

        var result = groups.ToList<TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Should().HaveCount(2);
        result[0].Category.Should().Be("Identity");
        result[0].Resources[0].Resource.Should().Be("Users");
        result[1].Category.Should().Be("Catalog");
        result[1].Resources[0].Resource.Should().Be("Products");
    }

    [Fact(DisplayName = "Should return empty list when PermissionGroup collection is empty")]
    public void ToList_EmptyCollection_ShouldReturnEmptyList()
    {
        var groups = Enumerable.Empty<PermissionGroup>();

        var result = groups.ToList<TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should map multiple PermissionGroups to list response preserving all items")]
    public void ToListResponse_MultipleCategories_ShouldMapResponse()
    {
        var groups = new[]
        {
            new PermissionGroup { Category = "Identity", Resources = [new ResourceGroup { ResourceName = "Users", Permissions = [PermissionMetadataMethod.For("admin", "identity", "users", "create")] }] },
            new PermissionGroup { Category = "Catalog", Resources = [new ResourceGroup { ResourceName = "Products", Permissions = [PermissionMetadataMethod.For("admin", "catalog", "products", "view")] }] }
        };

        var result = groups.ToListResponse<TestCategoryGroupList, TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Categories.Should().HaveCount(2);
        result.Categories[0].Category.Should().Be("Identity");
        result.Categories[1].Category.Should().Be("Catalog");
    }

    [Fact(DisplayName = "Should return empty categories list when PermissionGroup collection is empty")]
    public void ToListResponse_EmptyCollection_ShouldReturnEmptyCategories()
    {
        var groups = Enumerable.Empty<PermissionGroup>();

        var result = groups.ToListResponse<TestCategoryGroupList, TestCategoryGroupItem, TestResourceGroupItem, TestPermissionResponse>();

        result.Categories.Should().BeEmpty();
    }
}
