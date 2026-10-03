using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Shared.Application.Systems.SystemInfos;

namespace Shared.UnitTests.Application.Systems.SystemInfos;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "SystemInfo")]
public class SystemInfoTests
{
    private static Mock<IHostEnvironment> CreateMockEnvironment(string appName = "TestApp", string envName = "Development")
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.ApplicationName).Returns(appName);
        mock.Setup(e => e.EnvironmentName).Returns(envName);
        return mock;
    }

    private static IConfiguration CreateConfig(string? defaultCurrency = null)
    {
        var dict = new Dictionary<string, string?>();
        if (defaultCurrency is not null)
            dict["System:DefaultCurrency"] = defaultCurrency;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact(DisplayName = "SystemInfo should implement ISystemInfo")]
    public void SystemInfo_ShouldImplementISystemInfo()
    {
        var systemInfo = new SystemInfo(CreateMockEnvironment().Object, CreateConfig());

        systemInfo.Should().BeAssignableTo<ISystemInfo>();
    }

    [Fact(DisplayName = "ApplicationName should return application name from environment")]
    public void ApplicationName_ShouldReturnApplicationNameFromEnvironment()
    {
        var expectedName = "MyTestApp";
        var systemInfo = new SystemInfo(CreateMockEnvironment(appName: expectedName).Object, CreateConfig());

        var result = systemInfo.ApplicationName;

        result.Should().Be(expectedName);
    }

    [Fact(DisplayName = "ApplicationName should be null when environment returns null")]
    public void ApplicationName_NullFromEnvironment_ShouldBeNull()
    {
        var systemInfo = new SystemInfo(CreateMockEnvironment(appName: null!).Object, CreateConfig());

        var result = systemInfo.ApplicationName;

        result.Should().BeNull();
    }

    [Fact(DisplayName = "Environment should return environment name from environment")]
    public void Environment_ShouldReturnEnvironmentNameFromEnvironment()
    {
        var expectedEnv = "Production";
        var systemInfo = new SystemInfo(CreateMockEnvironment(envName: expectedEnv).Object, CreateConfig());

        var result = systemInfo.Environment;

        result.Should().Be(expectedEnv);
    }

    [Fact(DisplayName = "MachineName should return local machine name")]
    public void MachineName_ShouldReturnLocalMachineName()
    {
        var systemInfo = new SystemInfo(CreateMockEnvironment().Object, CreateConfig());

        var result = systemInfo.MachineName;

        result.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "ProcessId should return current process ID")]
    public void ProcessId_ShouldReturnCurrentProcessId()
    {
        var systemInfo = new SystemInfo(CreateMockEnvironment().Object, CreateConfig());

        var result = systemInfo.ProcessId;

        result.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "Version should return version string")]
    public void Version_ShouldReturnVersionString()
    {
        var systemInfo = new SystemInfo(CreateMockEnvironment().Object, CreateConfig());

        var result = systemInfo.Version;

        result.Should().NotBeNullOrEmpty();
    }
}
