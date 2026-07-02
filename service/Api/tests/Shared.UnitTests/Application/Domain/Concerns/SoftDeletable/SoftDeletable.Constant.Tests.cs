using Shared.Application.Domain.Concerns.SoftDeletable;

namespace Shared.UnitTests.Application.Domain.Concerns.SoftDeletable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class SoftDeletableConstantTests
{
    [Fact(DisplayName = "MaxDeletedByLength should be 100")]
    public void MaxDeletedByLength_ShouldBe100()
    {
        SoftDeletableConstant.Constraints.MaxDeletedByLength.Should().Be(100);
    }
}
