using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using Shared.Observability;
using Shared.Observability.Correlation;

namespace Shared.UnitTests.Observability;

[Trait("Category", "Unit")]
[Trait("Feature", "Observability")]
public class ObservabilityExtensionTests
{
    private static WebApplicationBuilder CreateBuilder(Action<ConfigurationBuilder>? configureConfig = null)
    {
        var configBuilder = new ConfigurationBuilder();
        configureConfig?.Invoke(configBuilder);
        var config = configBuilder.Build();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.Configuration.AddConfiguration(config);
        return builder;
    }

    [Fact(DisplayName = "AddObservability should register ICorrelationContext as scoped")]
    public void ShouldRegisterCorrelationContext()
    {
        var builder = CreateBuilder();
        builder.AddObservability();
        var sp = builder.Services.BuildServiceProvider();

        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();
        var ctx1 = scope1.ServiceProvider.GetRequiredService<ICorrelationContext>();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<ICorrelationContext>();

        ctx1.Should().NotBeNull();
        ctx2.Should().NotBeNull();
        ctx1.Should().NotBeSameAs(ctx2);
    }

    [Fact(DisplayName = "AddObservability should register health check service")]
    public void ShouldRegisterHealthChecks()
    {
        var builder = CreateBuilder();
        builder.AddObservability();

        var sp = builder.Services.BuildServiceProvider();
        var healthService = sp.GetService<HealthCheckService>();

        healthService.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddObservability should use defaults when no config section")]
    public void ShouldUseDefaultsWhenNoConfig()
    {
        var builder = CreateBuilder();
        builder.AddObservability();

        var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetRequiredService<ObservabilitySetting>();

        options.CorrelationHeader.Should().Be("X-Correlation-Id");
        options.ServiceName.Should().Be("ReSys.Api");
    }

    [Fact(DisplayName = "AddObservability should apply config override")]
    public void ShouldApplyConfigurationOverride()
    {
        var builder = CreateBuilder(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:CorrelationHeader"] = "X-Trace-Id",
                ["Observability:ServiceName"] = "MyService"
            });
        });
        builder.AddObservability();

        var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetRequiredService<ObservabilitySetting>();

        options.CorrelationHeader.Should().Be("X-Trace-Id");
        options.ServiceName.Should().Be("MyService");
    }

    [Fact(DisplayName = "AddObservability should return builder for chaining")]
    public void ShouldReturnBuilderForChaining()
    {
        var builder = CreateBuilder();
        var result = builder.AddObservability();
        result.Should().BeSameAs(builder);
    }

    [Fact(DisplayName = "AddObservability should set minimum log level")]
    public void ShouldSetMinimumLogLevel()
    {
        var builder = CreateBuilder(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:MinimumLogLevel"] = "Debug"
            });
        });
        builder.AddObservability();

        var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetRequiredService<ObservabilitySetting>();

        options.MinimumLogLevel.Should().Be(LogLevel.Debug);
    }
}
