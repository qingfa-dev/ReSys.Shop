using Microsoft.Extensions.Logging;

using Shared.Observability;

namespace Shared.UnitTests.Observability;

[Trait("Category", "Unit")]
[Trait("Feature", "Observability")]
public class ObservabilityConstantTests
{
    [Fact(DisplayName = "Defaults should have expected ServiceName")]
    public void Defaults_ShouldHaveExpectedServiceName()
    {
        ObservabilityConstant.Defaults.ServiceName.Should().Be("ReSys.Api");
    }

    [Fact(DisplayName = "Defaults should have expected CorrelationHeader")]
    public void Defaults_ShouldHaveExpectedCorrelationHeader()
    {
        ObservabilityConstant.Defaults.CorrelationHeader.Should().Be("X-Correlation-Id");
    }

    [Fact(DisplayName = "Defaults should have expected MinimumLogLevel")]
    public void Defaults_ShouldHaveExpectedMinimumLogLevel()
    {
        ObservabilityConstant.Defaults.MinimumLogLevel.Should().Be(LogLevel.Information);
    }

    [Fact(DisplayName = "Defaults should have sensitive headers")]
    public void Defaults_ShouldHaveSensitiveHeaders()
    {
        ObservabilityConstant.Defaults.SensitiveHeaders.Should().Contain("Authorization");
        ObservabilityConstant.Defaults.SensitiveHeaders.Should().Contain("Cookie");
        ObservabilityConstant.Defaults.SensitiveHeaders.Should().Contain("X-Api-Key");
    }

    [Fact(DisplayName = "Patterns should have valid CorrelationHeader regex")]
    public void Patterns_ShouldHaveValidCorrelationHeaderRegex()
    {
        var regex = new System.Text.RegularExpressions.Regex(
            ObservabilityConstant.Patterns.CorrelationHeader);
        regex.IsMatch("abc-123").Should().BeTrue();
    }
}
