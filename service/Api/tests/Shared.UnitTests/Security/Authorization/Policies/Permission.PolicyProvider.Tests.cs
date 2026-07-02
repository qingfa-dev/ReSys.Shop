using Microsoft.AspNetCore.Authorization;

using Shared.Security.Authorization.Policies;
using Shared.Security.Authorization.Registry;
using Shared.Security.Authorization.Requirements;

namespace Shared.UnitTests.Security.Authorization.Policies;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionPolicyProvider")]
public sealed class PermissionPolicyProviderTests
{
    [Fact(DisplayName = "PolicyProvider: should return PermissionRequirement for known permissions")]
    public async Task GetPolicyAsync_ShouldReturnPermissionRequirement_ForKnownPermission()
    {
        string knownPerm = PermissionContext.All[0].Identifier;
        PermissionPolicyProvider provider = new(
            Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions()));
        AuthorizationPolicy? policy = await provider.GetPolicyAsync(knownPerm);

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle(r => r is PermissionRequirement);
        ((PermissionRequirement)policy.Requirements.First(r => r is PermissionRequirement))
            .Permission.Should().Be(knownPerm);
    }

    [Fact(DisplayName = "PolicyProvider: should return null for unknown permissions (delegate to fallback)")]
    public async Task GetPolicyAsync_ShouldReturnNull_ForUnknownPermission()
    {
        PermissionPolicyProvider provider = new(
            Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions()));
        AuthorizationPolicy? policy = await provider.GetPolicyAsync("unknown.permission");

        policy.Should().BeNull();
    }

    [Fact(DisplayName = "PermissionPolicyProvider: GetDefaultPolicyAsync should delegate to default provider")]
    public async Task GetDefaultPolicyAsync_ShouldDelegateToDefaultProvider()
    {
        PermissionPolicyProvider provider = new(
            Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions()));
        AuthorizationPolicy result = await provider.GetDefaultPolicyAsync();

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PermissionPolicyProvider: GetFallbackPolicyAsync should delegate to default provider")]
    public async Task GetFallbackPolicyAsync_ShouldDelegateToDefaultProvider()
    {
        PermissionPolicyProvider provider = new(
            Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions()));
        AuthorizationPolicy? result = await provider.GetFallbackPolicyAsync();

        result.Should().BeNull();
    }
}
