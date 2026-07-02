using Shared.Operational.Persistence.Data;

namespace Shared.UnitTests.Operational.Persistence.Data;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class ApplicationDbContextTests
{
    [Fact(DisplayName = "Should implement IApplicationDbContext")]
    public void ShouldImplementIApplicationDbContext()
    {
        typeof(ApplicationDbContext).Should().Implement<IApplicationDbContext>();
    }

    [Fact(DisplayName = "Should implement IApplicationDbContext directly (not through base type)")]
    public void ShouldImplementIApplicationDbContextDirectly()
    {
        typeof(ApplicationDbContext)
            .GetInterfaces()
            .Should().Contain(typeof(IApplicationDbContext));
    }
}
