using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Cors;
using Shared.Security.Cors.Options;

namespace Shared.UnitTests.Security.Cors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Cors")]
public sealed class CorsExtensionsTests
{
    private static WebApplicationBuilder CreateBuilderWithCors(CorsSetting options)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var config = new Dictionary<string, string?>();
        for (int i = 0; i < options.Origins.Length; i++)
            config[$"Cors:Origins:{i}"] = options.Origins[i];
        config["Cors:AllowCredentials"] = options.AllowCredentials.ToString();
        builder.Configuration.AddInMemoryCollection(config);
        return builder;
    }

    [Fact(DisplayName = "AddCors should register CORS services")]
    public void AddCors_ShouldRegisterCorsServices()
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(new CorsSetting());
        builder.AddSecurityCors();

        using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        provider.GetService<ICorsService>()
            .Should()
            .NotBeNull();
    }

    [Fact(DisplayName = "Default policy should be added")]
    public async Task AddCors_ShouldAddDefaultPolicy()
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(new CorsSetting());
        builder.AddSecurityCors();

        await using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        ICorsPolicyProvider policyProvider =
            provider.GetRequiredService<ICorsPolicyProvider>();

        CorsPolicy? policy =
            await policyProvider.GetPolicyAsync(
                new DefaultHttpContext(),
                null);

        policy.Should().NotBeNull();
    }

    [Theory(DisplayName = "Explicit origins should be configured")]
    [InlineData("https://example.com")]
    [InlineData("https://shop.example.com")]
    [InlineData("https://admin.example.com")]
    public async Task ExplicitOrigins_ShouldBeConfigured(
        string origin)
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(
            new CorsSetting { Origins = [origin] });
        builder.AddSecurityCors();

        await using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        ICorsPolicyProvider policyProvider =
            provider.GetRequiredService<ICorsPolicyProvider>();

        CorsPolicy? policy =
            await policyProvider.GetPolicyAsync(
                new DefaultHttpContext(),
                null);

        policy.Should().NotBeNull();

        policy.AllowAnyOrigin.Should().BeFalse();

        policy.Origins.Should().Contain(origin);
    }

    [Theory(DisplayName = "Credentials should follow configuration")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Credentials_ShouldMatchConfiguration(
        bool allowCredentials)
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(
            new CorsSetting { Origins = ["https://example.com"], AllowCredentials = allowCredentials });
        builder.AddSecurityCors();

        await using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        ICorsPolicyProvider policyProvider =
            provider.GetRequiredService<ICorsPolicyProvider>();

        CorsPolicy? policy =
            await policyProvider.GetPolicyAsync(
                new DefaultHttpContext(),
                null);

        policy.Should().NotBeNull();

        policy.SupportsCredentials.Should()
            .Be(allowCredentials);
    }

    [Fact(DisplayName = "Wildcard origin should allow any origin")]
    public async Task WildcardOrigin_ShouldAllowAnyOrigin()
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(
            new CorsSetting { Origins = ["*"] });
        builder.AddSecurityCors();

        await using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        ICorsPolicyProvider policyProvider =
            provider.GetRequiredService<ICorsPolicyProvider>();

        CorsPolicy? policy =
            await policyProvider.GetPolicyAsync(
                new DefaultHttpContext(),
                null);

        policy.Should().NotBeNull();

        policy.AllowAnyOrigin.Should().BeTrue();
    }

    [Fact(DisplayName = "Wildcard origin should disable credentials")]
    public async Task WildcardOrigin_ShouldDisableCredentials()
    {
        WebApplicationBuilder builder = CreateBuilderWithCors(
            new CorsSetting { Origins = ["*"], AllowCredentials = true });
        builder.AddSecurityCors();

        await using ServiceProvider provider =
            builder.Services.BuildServiceProvider();

        ICorsPolicyProvider policyProvider =
            provider.GetRequiredService<ICorsPolicyProvider>();

        CorsPolicy? policy =
            await policyProvider.GetPolicyAsync(
                new DefaultHttpContext(),
                null);

        policy.Should().NotBeNull();

        policy.AllowAnyOrigin.Should().BeTrue();

        policy.SupportsCredentials.Should().BeFalse();
    }
}
