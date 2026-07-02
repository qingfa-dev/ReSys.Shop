using Shared.Application.Domain.Concerns.Parameterizable;

namespace Shared.UnitTests.Application.Domain.Concerns.Parameterizable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class ParameterizableConstantTests
{
    [Fact(DisplayName = "MaxNameLength should be 255")]
    public void MaxNameLength_ShouldBe255()
    {
        ParameterizableConstant.Constraints.MaxNameLength.Should().Be(255);
    }

    [Fact(DisplayName = "MaxPresentationLength should be 255")]
    public void MaxPresentationLength_ShouldBe255()
    {
        ParameterizableConstant.Constraints.MaxPresentationLength.Should().Be(255);
    }

    [Fact(DisplayName = "Defaults.Empty should be empty string")]
    public void DefaultsEmpty_ShouldBeEmptyString()
    {
        ParameterizableConstant.Defaults.Empty.Should().Be(string.Empty);
    }

    [Fact(DisplayName = "Defaults.Normalization.Separator should be hyphen")]
    public void DefaultsNormalizationSeparator_ShouldBeHyphen()
    {
        ParameterizableConstant.Defaults.Normalization.Separator.Should().Be('-');
    }
}
