using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Authentication;
using Shared.Security.Authentication.Contexts.Services;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.Tokens.Services.Access;

namespace Shared.UnitTests.Security.Authentication;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "AuthRegistration")]
public sealed class AuthenticationExtensionTests
{
    private static WebApplicationBuilder CreateBuilderWithConfig()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:Secret"] = "the-krabby-patty-secret-formula-is-mine-32!",
            ["JwtSettings:Issuer"] = "test-issuer",
            ["JwtSettings:Audience"] = "test-audience",
            ["Authentication:Google:ClientId"] = "test-client-id"
        });
        builder.AddApplicationAuthentication();
        return builder;
    }

    [Fact(DisplayName = "AddApplicationAuthentication should register ICurrentUser from context module")]
    public void ShouldRegisterCurrentUser()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(ICurrentUser) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddApplicationAuthentication should register IAccessTokenService from tokens module")]
    public void ShouldRegisterAccessTokenService()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IAccessTokenService) &&
            s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddApplicationAuthentication should register IExternalLoginProvider from external module")]
    public void ShouldRegisterExternalLoginProvider()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IExternalLoginProvider) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }
}
