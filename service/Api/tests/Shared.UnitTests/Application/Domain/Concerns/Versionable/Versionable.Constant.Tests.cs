using Shared.Application.Domain.Concerns.Versionable;

namespace Shared.UnitTests.Application.Domain.Concerns.Versionable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class VersionableConstantTests
{
    [Fact(DisplayName = "InitialVersion constant should be 1")]
    public void InitialVersion_ShouldBeOne()
    {
        VersionableConstant.Defaults.InitialVersion.Should().Be(1);
    }
}
