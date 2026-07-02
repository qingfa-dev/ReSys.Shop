using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Authentication.Contexts;
using Shared.Security.Authentication.Contexts.Services;

namespace Shared.UnitTests.Security.Authentication.Context;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "HttpContexts")]
public sealed class ContextExtensionTests
{
    [Fact(DisplayName = "AddAuthenticationContext should register IHttpContextAccessor as singleton")]
    public void AddAuthenticationContext_ShouldRegisterHttpContextAccessor()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        // Act
        builder.AddAuthenticationContext();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(IHttpContextAccessor) &&
            s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddAuthenticationContext should register ICurrentUser as scoped")]
    public void AddAuthenticationContext_ShouldRegisterCurrentUser()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        // Act
        builder.AddAuthenticationContext();

        // Assert
        builder.Services.Should().ContainSingle(s =>
            s.ServiceType == typeof(ICurrentUser) &&
            s.ImplementationType == typeof(CurrentUser) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }
}
