using Shared.Observability;

namespace Shared.UnitTests.Observability;

[Trait("Category", "Unit")]
[Trait("Feature", "Observability")]
public class ObservabilitySettingResultTests
{
    [Fact(DisplayName = "CorrelationHeaderEmpty should have correct code")]
    public void CorrelationHeaderEmpty_ShouldHaveCorrectCode()
    {
        ObservabilitySettingResult.Failure.CorrelationHeaderEmpty.Code
            .Should().Be("Observability.CorrelationHeader.Empty");
    }

    [Fact(DisplayName = "CorrelationHeaderInvalid should have correct code")]
    public void CorrelationHeaderInvalid_ShouldHaveCorrectCode()
    {
        ObservabilitySettingResult.Failure.CorrelationHeaderInvalid.Code
            .Should().Be("Observability.CorrelationHeader.Invalid");
    }

    [Fact(DisplayName = "ServiceNameEmpty should have correct code")]
    public void ServiceNameEmpty_ShouldHaveCorrectCode()
    {
        ObservabilitySettingResult.Failure.ServiceNameEmpty.Code
            .Should().Be("Observability.ServiceName.Empty");
    }
}
