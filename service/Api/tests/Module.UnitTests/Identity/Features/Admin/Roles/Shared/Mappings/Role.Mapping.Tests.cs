using Module.Identity.Features.Admin.Shared.Mappings;
using Module.Identity.Features.Admin.Shared.Models;

using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RoleMapping")]
public class RoleMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var request = new RoleRequest
        {
            Name = "Admin",
            Description = "Administrator role",
        };

        var role = request.MapToDomain();

        role.Should().NotBeNull();
        role.Name.Should().Be(request.Name);
        role.Description.Should().Be(request.Description);
        role.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "ToDomain: Should handle null description")]
    public void ToDomain_WhenDescriptionIsNull_ShouldMapCorrectly()
    {
        var request = new RoleRequest
        {
            Name = "Editor",
            Description = null,
        };

        var role = request.MapToDomain();

        role.Name.Should().Be("Editor");
        role.Description.Should().BeNull();
    }

    [Fact(DisplayName = "ToDomain (Update): Should update existing entity from request")]
    public void ToDomain_Update_ShouldUpdateEntity()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Old Role",
            Description = "Old description",
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var request = new RoleRequest
        {
            Name = "Updated Role",
            Description = "Updated description",
        };

        request.MapToDomain(role);

        role.Name.Should().Be("Updated Role");
        role.Description.Should().Be("Updated description");
        role.ModifiedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var role = CreateRole();

        var response = role.MapToDetail<RoleDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(role.Id);
        response.Name.Should().Be(role.Name);
        response.Description.Should().Be(role.Description);
        response.IsSystem.Should().Be(role.IsSystem);
        response.CreatedAtUtc.Should().Be(role.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(role.ModifiedAtUtc);
        response.CreatedBy.Should().Be(role.CreatedBy);
        response.ModifiedBy.Should().Be(role.ModifiedBy);
    }

    [Fact(DisplayName = "ToDetail: Should handle null Name as empty string")]
    public void ToDetail_WhenNameIsNull_ShouldUseEmptyString()
    {
        var role = CreateRole(r => r.Name = null);

        var response = role.MapToDetail<RoleDetailResponse>();

        response.Name.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToDetail: Should handle null auditable fields")]
    public void ToDetail_WhenAuditableFieldsAreNull_ShouldMapCorrectly()
    {
        var role = CreateRole(r =>
        {
            r.Description = null;
            r.ModifiedAtUtc = null;
            r.CreatedBy = null;
            r.ModifiedBy = null;
        });

        var response = role.MapToDetail<RoleDetailResponse>();

        response.Description.Should().BeNull();
        response.ModifiedAtUtc.Should().BeNull();
        response.CreatedBy.Should().BeNull();
        response.ModifiedBy.Should().BeNull();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var role = CreateRole();

        var response = role.MapToListItem<RoleListResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(role.Id);
        response.Name.Should().Be(role.Name);
        response.Description.Should().Be(role.Description);
        response.IsSystem.Should().Be(role.IsSystem);
    }

    [Fact(DisplayName = "ToListItem: Should handle null Name as empty string")]
    public void ToListItem_WhenNameIsNull_ShouldUseEmptyString()
    {
        var role = CreateRole(r =>
        {
            r.Name = null;
            r.Description = null;
        });

        var response = role.MapToListItem<RoleListResponse>();

        response.Name.Should().BeEmpty();
        response.Description.Should().BeNull();
    }

    private static Role CreateRole(Action<Role>? configure = null)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Description = "Can manage products",
            IsSystem = false,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-30),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "admin",
            ModifiedBy = "admin",
        };
        configure?.Invoke(role);
        return role;
    }
}
