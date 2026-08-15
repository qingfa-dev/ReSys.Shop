using Module.Identity.Features.Shared.Admin.Permissions.Shared.Mappings;
using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

using Shared.Security.Identity.Domain.Permissions;

namespace Module.UnitTests.Identity.Features.Admin.Permissions.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Permissions/CompositeMapping")]
public class PermissionCompositeMappingTests
{
    private sealed record TestPermissionItem : PermissionAssignmentItemResponse;
    private sealed record TestResourceItem : ResourceGroupListItemResponse<TestPermissionItem>;
    private sealed record TestCategoryItem : CategoryGroupListItemResponse<TestResourceItem>;
    private sealed record TestComposite : CategoryGroupListResponse<TestCategoryItem, TestResourceItem>;

    private static IReadOnlyList<PermissionMetadata> SamplePermissions() =>
    [
        PermissionMetadataMethod.For("admin", "identity", "users", "view"),
        PermissionMetadataMethod.For("admin", "identity", "users", "create"),
        PermissionMetadataMethod.For("admin", "identity", "roles", "view"),
        PermissionMetadataMethod.For("admin", "catalog", "products", "create")
    ];

    [Fact(DisplayName = "Should group permissions by category then resource and map all permission fields")]
    public void MapToPermissionComposite_ShouldGroupByCategoryAndResource()
    {
        var assigned = new HashSet<string> { "admin.identity.users.view" };

        var result = SamplePermissions()
            .MapToPermissionComposite<TestComposite, TestCategoryItem, TestResourceItem, TestPermissionItem>(assigned);

        result.Categories.Should().HaveCount(2);
        result.Categories[0].Category.Should().Be("identity");
        result.Categories[0].Resources.Should().HaveCount(2);
        result.Categories[0].Resources[0].Resource.Should().Be("users");
        result.Categories[0].Resources[0].Permissions.Should().HaveCount(2);
        result.Categories[0].Resources[1].Resource.Should().Be("roles");
        result.Categories[1].Category.Should().Be("catalog");
        result.Categories[1].Resources.Should().ContainSingle();
        result.Categories[1].Resources[0].Resource.Should().Be("products");
    }

    [Fact(DisplayName = "Should map each permission's Identifier, Name, Action and IsAssigned")]
    public void MapToPermissionComposite_ShouldMapPermissionFieldsAndAssignment()
    {
        var assigned = new HashSet<string> { "admin.identity.users.create", "admin.catalog.products.create" };

        var result = SamplePermissions()
            .MapToPermissionComposite<TestComposite, TestCategoryItem, TestResourceItem, TestPermissionItem>(assigned);

        var users = result.Categories[0].Resources[0].Permissions;
        users.Should().ContainSingle(p => p.Identifier == "admin.identity.users.view" && !p.IsAssigned);
        var assignedUser = users.Should().ContainSingle(p => p.Identifier == "admin.identity.users.create").Which;
        assignedUser.IsAssigned.Should().BeTrue();
        assignedUser.Name.Should().Be("create users");
        assignedUser.Action.Should().Be("create");

        var products = result.Categories[1].Resources[0].Permissions.Single();
        products.IsAssigned.Should().BeTrue();
    }

    [Fact(DisplayName = "Should leave every permission unassigned when the assigned set is empty")]
    public void MapToPermissionComposite_EmptyAssignedSet_ShouldLeaveAllUnassigned()
    {
        var result = SamplePermissions()
            .MapToPermissionComposite<TestComposite, TestCategoryItem, TestResourceItem, TestPermissionItem>(new HashSet<string>());

        result.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions)
            .Should().AllSatisfy(p => p.IsAssigned.Should().BeFalse());
    }
}
