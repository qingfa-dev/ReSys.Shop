using Module.Identity.Features.Store.Auth.Login.External.Providers;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.External.Services;

namespace Module.UnitTests.Identity.Features.Store.Auth.Login.External.Providers;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Logins")]
public class ExternalProvidersTests
{
    [Fact(DisplayName = "Should return providers from discovery service")]
    public async Task Handle_WithProviders_ReturnsProviderConfigs()
    {
        var provider1Mock = new Mock<IExternalLoginProvider>();
        provider1Mock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "google",
                Options = new Dictionary<string, string> { ["client_id"] = "id1" }
            });

        var provider2Mock = new Mock<IExternalLoginProvider>();
        provider2Mock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "facebook",
                Options = new Dictionary<string, string> { ["client_id"] = "id2" }
            });

        var service = new ExternalProviderRegistry(new[] { provider1Mock.Object, provider2Mock.Object });
        var handler = new ExternalProviders.QueryHandler(service);

        var result = await handler.Handle(new ExternalProviders.PagedQuery(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Provider == "google");
        result.Items.Should().Contain(p => p.Provider == "facebook");
    }

    [Fact(DisplayName = "Should return empty list when no providers configured")]
    public async Task Handle_NoProviders_ReturnsEmptyList()
    {
        var service = new ExternalProviderRegistry(Enumerable.Empty<IExternalLoginProvider>());
        var handler = new ExternalProviders.QueryHandler(service);

        var result = await handler.Handle(new ExternalProviders.PagedQuery(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}

