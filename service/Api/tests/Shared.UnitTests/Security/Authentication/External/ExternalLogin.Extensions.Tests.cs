using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Authentication.External;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.External.Providers.Google;
using Shared.Security.Authentication.External.Services;

namespace Shared.UnitTests.Security.Authentication.External;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "ExternalAuth")]
public sealed class ExternalLoginExtensionsTests
{
    private static WebApplicationBuilder CreateBuilderWithConfig()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Google:ClientId"] = "test-client-id"
        });
        builder.AddExternalAuthentication();
        return builder;
    }

    [Fact(DisplayName = "AddExternalAuthentication should register IGoogleTokenValidator as scoped")]
    public void ShouldRegisterGoogleTokenValidator()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IGoogleTokenValidator) &&
            s.ImplementationType == typeof(GoogleTokenValidator) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddExternalAuthentication should register IExternalLoginProvider as scoped")]
    public void ShouldRegisterExternalLoginProvider()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IExternalLoginProvider) &&
            s.ImplementationType == typeof(GoogleExternalProvider) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddExternalAuthentication should register ExternalProviderDiscoveryService as scoped")]
    public void ShouldRegisterExternalProviderDiscoveryService()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(ExternalProviderRegistry) &&
            s.ImplementationType == typeof(ExternalProviderRegistry) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }
}
