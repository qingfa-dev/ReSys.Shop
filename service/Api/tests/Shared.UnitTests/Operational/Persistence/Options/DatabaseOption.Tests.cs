using ReSys.ServiceDefaults.Constants;

using Shared.Operational.Persistence.Options;

namespace Shared.UnitTests.Operational.Persistence.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DatabaseOptionTests
{
    [Fact(DisplayName = "Default connection string should be 'DefaultConnection'")]
    public void Default_ShouldBeDefaultConnection()
    {
        DatabaseOption.Default.Should().Be("DefaultConnection");
    }

    [Fact(DisplayName = "Aspire connection string should match ServiceDefaults constant")]
    public void Aspire_ShouldMatchServiceDefaultsConstant()
    {
        DatabaseOption.Aspire.Should().Be(Infrastructures.Databases.Resource);
    }

    [Fact(DisplayName = "InMemory connection string should be 'InMemoryDatabase'")]
    public void InMemory_ShouldBeInMemoryDatabase()
    {
        DatabaseOption.InMemory.Should().Be("InMemoryDatabase");
    }
}
