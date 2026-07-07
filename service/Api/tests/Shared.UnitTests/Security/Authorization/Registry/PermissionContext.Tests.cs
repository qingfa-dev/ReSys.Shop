using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.UnitTests.Security.Authorization.Registry;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionContext")]
public sealed class PermissionContextTests
{
    [Fact(DisplayName = "PermissionContext: Domains should have all expected entries")]
    public void Domains_ShouldHaveAllEntries()
    {
        PermissionContext.Domains.Admin.Value.Should().Be("admin");
        PermissionContext.Domains.Catalog.Value.Should().Be("catalog");
        PermissionContext.Domains.Identity.Value.Should().Be("identity");
        PermissionContext.Domains.Ordering.Value.Should().Be("ordering");
        PermissionContext.Domains.Inventory.Value.Should().Be("inventory");
        PermissionContext.Domains.Configuration.Value.Should().Be("configuration");
        PermissionContext.Domains.Dashboard.Value.Should().Be("dashboard");
    }

    [Fact(DisplayName = "PermissionContext: Categories should have expected entries")]
    public void Categories_ShouldHaveExpectedEntries()
    {
        PermissionContext.Categories.Location.Value.Should().Be("location");
        PermissionContext.Categories.Identity.Value.Should().Be("identity");
        PermissionContext.Categories.Profile.Value.Should().Be("profile");
        PermissionContext.Categories.Catalog.Value.Should().Be("catalog");
        PermissionContext.Categories.Ordering.Value.Should().Be("ordering");
        PermissionContext.Categories.Inventory.Value.Should().Be("inventory");
        PermissionContext.Categories.Configuration.Value.Should().Be("configuration");
        PermissionContext.Categories.Merchandising.Value.Should().Be("merchandising");
        PermissionContext.Categories.Dashboard.Value.Should().Be("dashboard");
    }

    [Fact(DisplayName = "PermissionContext: Resources should have expected entries")]
    public void Resources_ShouldHaveExpectedEntries()
    {
        PermissionContext.Resources.Users.Value.Should().Be("users");
        PermissionContext.Resources.Products.Value.Should().Be("products");
        PermissionContext.Resources.Orders.Value.Should().Be("orders");
        PermissionContext.Resources.StockItems.Value.Should().Be("stockitems");
        PermissionContext.Resources.Settings.Value.Should().Be("settings");
        PermissionContext.Resources.Sales.Value.Should().Be("sales");
        PermissionContext.Resources.Stores.Value.Should().Be("stores");
    }

    [Fact(DisplayName = "PermissionContext: All should contain permissions from all modules")]
    public void All_ShouldContainAllModulePermissions()
    {
        PermissionContext.All.Should().NotBeNullOrEmpty();
        PermissionContext.All.Should().HaveCountGreaterThan(10);
    }

    [Fact(DisplayName = "PermissionContext: IsKnown should return true for known identifiers")]
    public void IsKnown_ShouldReturnTrue_ForKnownIdentifier()
    {
        string identifier = PermissionContext.All[0].Identifier;
        PermissionContext.IsKnown(identifier).Should().BeTrue();
    }

    [Fact(DisplayName = "PermissionContext: IsKnown should return false for unknown identifiers")]
    public void IsKnown_ShouldReturnFalse_ForUnknown()
    {
        PermissionContext.IsKnown("nonexistent.permission").Should().BeFalse();
    }

    [Fact(DisplayName = "PermissionContext: ByIdentifier should return correct PermissionMetadata")]
    public void ByIdentifier_ShouldReturnCorrectPermissionMetadata()
    {
        PermissionMetadata reference = PermissionContext.All[0];
        PermissionMetadata? result = PermissionContext.ByIdentifier(reference.Identifier);
        result.Should().NotBeNull();
        result!.Identifier.Should().Be(reference.Identifier);
    }

    [Fact(DisplayName = "PermissionContext: ByIdentifier should return null for unknown")]
    public void ByIdentifier_ShouldReturnNull_ForUnknown()
    {
        PermissionContext.ByIdentifier("unknown").Should().BeNull();
    }

    [Fact(DisplayName = "PermissionContext: ByCategory should group permissions")]
    public void ByCategory_ShouldGroupPermissions()
    {
        IReadOnlyList<PermissionMetadata> catalogPerms = PermissionContext.ByCategory("catalog");
        catalogPerms.Should().NotBeNullOrEmpty();
        catalogPerms.Should().AllSatisfy(p => p.Category.Should().Be("catalog"));
    }
}
