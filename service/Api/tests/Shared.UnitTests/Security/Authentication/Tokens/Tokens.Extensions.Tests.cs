using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;

namespace Shared.UnitTests.Security.Authentication.Tokens;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenServiceRegistration")]
public sealed class TokensExtensionsTests
{
    private static WebApplicationBuilder CreateBuilderWithConfig()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddTokenAuthentication();
        return builder;
    }

    [Fact(DisplayName = "AddTokenAuthentication should register IRefreshTokenStore as scoped")]
    public void ShouldRegisterRefreshTokenStore()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IRefreshTokenStore) &&
            s.ImplementationType == typeof(RefreshTokenStore) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddTokenAuthentication should register ITokenTheftDetector as scoped")]
    public void ShouldRegisterTokenTheftDetector()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(ITokenTheftDetector) &&
            s.ImplementationType == typeof(TokenTheftDetector) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddTokenAuthentication should register ITokenBlacklistService as singleton")]
    public void ShouldRegisterTokenBlacklistService()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(ITokenBlacklistService) &&
            s.ImplementationType == typeof(TokenBlacklistService) &&
            s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddTokenAuthentication should register IAccessTokenService as singleton")]
    public void ShouldRegisterAccessTokenService()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IAccessTokenService) &&
            s.ImplementationType == typeof(AccessTokenService) &&
            s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddTokenAuthentication should register IRefreshTokenService as scoped")]
    public void ShouldRegisterRefreshTokenService()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IRefreshTokenService) &&
            s.ImplementationType == typeof(RefreshTokenService) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact(DisplayName = "AddTokenAuthentication should register JwtSettings options")]
    public void ShouldRegisterJwtSettingsOptions()
    {
        // Arrange
        WebApplicationBuilder builder = CreateBuilderWithConfig();

        // Assert
        builder.Services.Should().Contain(s =>
            s.ServiceType == typeof(IConfigureOptions<JwtSettings>));
    }
}
