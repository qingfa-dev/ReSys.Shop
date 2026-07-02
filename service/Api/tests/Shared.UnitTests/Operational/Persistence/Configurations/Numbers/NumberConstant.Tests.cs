using Shared.Operational.Persistence.Configurations.Numbers;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Numbers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class NumberConstantTests
{
    public class Constraints
    {
        [Fact(DisplayName = "Should have correct DecimalPrecision value")]
        public void DecimalPrecision_ShouldBe18()
        {
            NumberConstant.Constraints.DecimalPrecision.Should().Be(18);
        }

        [Fact(DisplayName = "Should have correct DecimalScale value")]
        public void DecimalScale_ShouldBe2()
        {
            NumberConstant.Constraints.DecimalScale.Should().Be(2);
        }
    }
}
