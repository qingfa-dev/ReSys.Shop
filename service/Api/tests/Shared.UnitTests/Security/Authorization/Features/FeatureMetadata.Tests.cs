using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.UnitTests.Security.Authorization.Features;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "FeatureMetadata")]
public sealed class FeatureMetadataTests
{
    public static TheoryData<Func<IReadOnlyList<PermissionMetadata>>, string> ModuleData => new()
    {
        { () => CatalogFeatureMetadata.All, CatalogFeatureMetadata.ModuleName },
        { () => IdentityFeatureMetadata.All, IdentityFeatureMetadata.ModuleName },
        { () => LocationFeatureMetadata.All, LocationFeatureMetadata.ModuleName },
        { () => ProfileFeatureMetadata.All, ProfileFeatureMetadata.ModuleName },
        { () => OrderingFeatureMetadata.All, OrderingFeatureMetadata.ModuleName },
        { () => InventoryFeatureMetadata.All, InventoryFeatureMetadata.ModuleName },
        { () => ConfigurationFeatureMetadata.All, ConfigurationFeatureMetadata.ModuleName },
        { () => PromotionsFeatureMetadata.All, PromotionsFeatureMetadata.ModuleName },
        { () => DashboardFeatureMetadata.All, DashboardFeatureMetadata.ModuleName },
    };

    [Theory(DisplayName = "FeatureMetadata: module should have non-empty All and matching ModuleName")]
    [MemberData(nameof(ModuleData))]
    public void Module_ShouldHaveNonEmptyPermissions_AndMatchingModuleName(
        Func<IReadOnlyList<PermissionMetadata>> getAll, string moduleName)
    {
        IReadOnlyList<PermissionMetadata> permissions = getAll();
        permissions.Should().NotBeNullOrEmpty();
        permissions.Should().AllSatisfy(p =>
        {
            p.Identifier.Should().NotBeNullOrWhiteSpace();
            p.Identifier.Split('.').Should().HaveCountGreaterThanOrEqualTo(4);
        });
        moduleName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "FeatureMetadata: all modules should have unique identifiers (no duplicates across modules)")]
    public void AllModules_ShouldHaveUniqueIdentifiers()
    {
        List<string> allIdentifiers =
        [
            .. CatalogFeatureMetadata.All.Select(static p => p.Identifier),
            .. IdentityFeatureMetadata.All.Select(static p => p.Identifier),
            .. LocationFeatureMetadata.All.Select(static p => p.Identifier),
            .. ProfileFeatureMetadata.All.Select(static p => p.Identifier),
            .. OrderingFeatureMetadata.All.Select(static p => p.Identifier),
            .. InventoryFeatureMetadata.All.Select(static p => p.Identifier),
            .. ConfigurationFeatureMetadata.All.Select(static p => p.Identifier),
            .. PromotionsFeatureMetadata.All.Select(static p => p.Identifier),
            .. DashboardFeatureMetadata.All.Select(static p => p.Identifier),
        ];

        allIdentifiers.Should().OnlyHaveUniqueItems("because permission identifiers must be unique across all modules");
    }
}
