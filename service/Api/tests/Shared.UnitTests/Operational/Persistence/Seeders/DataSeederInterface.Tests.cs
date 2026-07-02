using Shared.Operational.Persistence.Seeders;

namespace Shared.UnitTests.Operational.Persistence.Seeders;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DataSeederInterfaceTests
{
    [Fact(DisplayName = "IDataSeeder should define SeedAsync method returning Task<Result>")]
    public void IDataSeeder_ShouldDefineSeedAsyncMethod()
    {
        typeof(IDataSeeder)
            .GetMethod(nameof(IDataSeeder.SeedAsync))
            .Should().NotBeNull();
        typeof(IDataSeeder)
            .GetMethod(nameof(IDataSeeder.SeedAsync))
            !.ReturnType.Should().Be<Task<Result>>();
    }

    [Fact(DisplayName = "IDataSeeder should define Order property returning int")]
    public void IDataSeeder_ShouldDefineOrderProperty()
    {
        typeof(IDataSeeder)
            .GetProperty(nameof(IDataSeeder.Order))
            .Should().NotBeNull();
        typeof(IDataSeeder)
            .GetProperty(nameof(IDataSeeder.Order))
            !.PropertyType.Should().Be<Int32>();
    }
}
