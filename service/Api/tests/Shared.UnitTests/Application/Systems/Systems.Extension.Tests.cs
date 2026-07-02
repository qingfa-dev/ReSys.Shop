using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shared.Application.Systems;
using Shared.Application.Systems.SystemDateTimes;
using Shared.Application.Systems.SystemInfos;

namespace Shared.UnitTests.Application.Systems;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Systems")]
public class SystemsExtensionsTests
{
    [Fact(DisplayName = "AddSystems should register ISystemDateTime as singleton")]
    public void AddSystems_ShouldRegisterISystemDateTimeAsSingleton()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddSystems();

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(ISystemDateTime));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddSystems should register ISystemInfo as singleton")]
    public void AddSystems_ShouldRegisterISystemInfoAsSingleton()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddSystems();

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(ISystemInfo));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "AddSystems should resolve ISystemDateTime")]
    public void AddSystems_ShouldResolveISystemDateTime()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddSystems();

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        ISystemDateTime? systemDateTime = provider.GetService<ISystemDateTime>();

        systemDateTime.Should().NotBeNull();
        systemDateTime.Should().BeOfType<SystemDateTime>();
    }

    [Fact(DisplayName = "AddSystems should resolve ISystemInfo")]
    public void AddSystems_ShouldResolveISystemInfo()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.ApplicationName).Returns("TestApp");
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        builder.Services.AddSingleton(mockEnvironment.Object);
        builder.AddSystems();

        ServiceProvider provider = builder.Services.BuildServiceProvider();
        ISystemInfo? systemInfo = provider.GetService<ISystemInfo>();

        systemInfo.Should().NotBeNull();
        systemInfo.Should().BeOfType<SystemInfo>();
    }

    [Fact(DisplayName = "AddSystems should return builder for chaining")]
    public void AddSystems_ShouldReturnBuilderForChaining()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        WebApplicationBuilder result = builder.AddSystems();

        result.Should().BeSameAs(builder);
    }
}
