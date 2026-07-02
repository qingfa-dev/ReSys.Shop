using FluentValidation.TestHelper;

using Shared.Observability;

namespace Shared.UnitTests.Observability;

[Trait("Category", "Unit")]
[Trait("Feature", "Observability")]
public class ObservabilitySettingValidatorTests
{
    private readonly ObservabilitySettingValidator _sut = new();

    [Fact(DisplayName = "When CorrelationHeader empty should fail")]
    public void WhenCorrelationHeaderEmpty_ShouldFail()
    {
        var setting = new ObservabilitySetting { CorrelationHeader = string.Empty };

        var result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.CorrelationHeader);
    }

    [Fact(DisplayName = "When CorrelationHeader has invalid characters should fail")]
    public void WhenCorrelationHeaderInvalidChars_ShouldFail()
    {
        var setting = new ObservabilitySetting { CorrelationHeader = "X Correlation ID" };

        var result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.CorrelationHeader);
    }

    [Fact(DisplayName = "When CorrelationHeader has valid header should pass")]
    public void WhenCorrelationHeaderValid_ShouldPass()
    {
        var setting = new ObservabilitySetting { CorrelationHeader = "X-Request-Id" };

        var result = _sut.TestValidate(setting);

        result.ShouldNotHaveValidationErrorFor(x => x.CorrelationHeader);
    }

    [Fact(DisplayName = "When ServiceName empty should fail")]
    public void WhenServiceNameEmpty_ShouldFail()
    {
        var setting = new ObservabilitySetting { ServiceName = string.Empty };

        var result = _sut.TestValidate(setting);

        result.ShouldHaveValidationErrorFor(x => x.ServiceName);
    }

    [Fact(DisplayName = "When all valid should pass")]
    public void WhenAllValid_ShouldPass()
    {
        var setting = new ObservabilitySetting();

        var result = _sut.TestValidate(setting);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
