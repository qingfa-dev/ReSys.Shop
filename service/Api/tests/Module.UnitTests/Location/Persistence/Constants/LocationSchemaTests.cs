using Module.Location.Persistence;

namespace Module.UnitTests.Location.Persistence.Constants;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Schema")]
public class LocationSchemaTests
{
    public class TableNames
    {
        [Fact]
        public void ShouldUseToSnakeCase_ForAllEntries()
        {
            LocationSchema.TableNames.Countries.Should().Be("countries");
            LocationSchema.TableNames.States.Should().Be("states");
        }
    }

    public class Name
    {
        [Fact]
        public void ShouldUseToSnakeCase()
        {
            LocationSchema.Name.Should().Be("location");
        }
    }
}
