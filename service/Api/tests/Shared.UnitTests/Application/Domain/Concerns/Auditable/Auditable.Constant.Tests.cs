using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.UnitTests.Application.Domain.Concerns.Auditable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class AuditableConstantTests
{
    [Fact(DisplayName = "MaxCreatedByLength should be 100")]
    public void MaxCreatedByLength_ShouldBe100()
    {
        AuditableConstant.Constraints.MaxCreatedByLength.Should().Be(100);
    }

    [Fact(DisplayName = "MaxModifiedByLength should be 100")]
    public void MaxModifiedByLength_ShouldBe100()
    {
        AuditableConstant.Constraints.MaxModifiedByLength.Should().Be(100);
    }
}
