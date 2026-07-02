using Shared.Operational.Persistence.Configurations.Enums;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Enums;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class EnumConstantTests
{
    public class Constraints
    {
        [Fact(DisplayName = "Should have correct MaxLength value")]
        public void MaxLength_ShouldBe100()
        {
            EnumConstant.Constraints.MaxLength.Should().Be(100);
        }
    }
}
