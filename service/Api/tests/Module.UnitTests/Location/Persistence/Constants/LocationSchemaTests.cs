using Module.Location.Persistence.Constants;

namespace Module.UnitTests.Location.Persistence.Constants;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Schema")]
public class LocationSchemaTests
{
    public class TableNames
    {
        [Fact]
        public void ShouldUseToSnakeCase_ForAllEntries()
        {
            LocationSchema.TableNames.Countries.Should().Be("country");
            LocationSchema.TableNames.States.Should().Be("state");
        }
    }

    public class Name
    {
        [Fact]
        public void ShouldUseToSnakeCase()
        {
            LocationSchema.Name.Should().Be("locations");
        }
    }
}
