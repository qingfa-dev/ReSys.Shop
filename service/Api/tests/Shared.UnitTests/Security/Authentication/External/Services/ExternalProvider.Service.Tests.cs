using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.External.Services;

namespace Shared.UnitTests.Security.Authentication.External.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "ExternalAuth")]
public sealed class ExternalProviderDiscoveryServiceTests
{
    [Fact(DisplayName = "GetAvailableProviders should return empty result when no providers registered")]
    public void GetAvailableProviders_WithNoProviders_ReturnsEmptyResult()
    {
        // Arrange
        ExternalProviderRegistry service = new(Enumerable.Empty<IExternalLoginProvider>());

        // Act
        PagedResult<ProviderOption> result = service.GetAvailableProviders();

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "GetAvailableProviders should return single provider when one is registered")]
    public void GetAvailableProviders_WithOneProvider_ReturnsSingleResult()
    {
        // Arrange
        Mock<IExternalLoginProvider> providerMock = new();
        providerMock.Setup(p => p.Provider).Returns("google");
        providerMock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "google",
                Options = new Dictionary<string, string> { ["client_id"] = "test-id" }
            });

        ExternalProviderRegistry service = new([providerMock.Object]);

        // Act
        PagedResult<ProviderOption> result = service.GetAvailableProviders();

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.ElementAt(0).Provider.Should().Be("google");
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "GetAvailableProviders should return multiple providers when multiple are registered")]
    public void GetAvailableProviders_WithMultipleProviders_ReturnsAllProviders()
    {
        // Arrange
        Mock<IExternalLoginProvider> googleMock = new();
        googleMock.Setup(p => p.Provider).Returns("google");
        googleMock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "google",
                Options = new Dictionary<string, string>()
            });

        Mock<IExternalLoginProvider> appleMock = new();
        appleMock.Setup(p => p.Provider).Returns("apple");
        appleMock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "apple",
                Options = new Dictionary<string, string>()
            });

        ExternalProviderRegistry service = new([googleMock.Object, appleMock.Object]);

        // Act
        PagedResult<ProviderOption> result = service.GetAvailableProviders();

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Provider).Should().BeEquivalentTo(["google", "apple"]);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "GetAvailableProviders should use correct page size from total count")]
    public void GetAvailableProviders_ShouldSetCorrectPageSize()
    {
        // Arrange
        Mock<IExternalLoginProvider> providerMock = new();
        providerMock.Setup(p => p.GetProviderConfig()).Returns(
            new ProviderOption
            {
                Provider = "google",
                Options = new Dictionary<string, string>()
            });

        ExternalProviderRegistry service = new([providerMock.Object]);

        // Act
        PagedResult<ProviderOption> result = service.GetAvailableProviders();

        // Assert
        result.PageSize.Should().Be(1);
    }
}
