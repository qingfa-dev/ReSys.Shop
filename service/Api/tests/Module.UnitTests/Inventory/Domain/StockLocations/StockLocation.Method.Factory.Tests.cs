using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationMethodFactoryTests
{
    [Fact(DisplayName = "Create: Should return StockLocation with correct properties")]
    public void Create_WithValidParameters_ShouldReturnStockLocation()
    {
        var id = Guid.NewGuid();

        var result = StockLocationMethod.Create("Warehouse A", id: id);
        var location = result.Value;

        result.IsSuccess.Should().BeTrue();
        location.Id.Should().Be(id);
        location.Name.Should().Be("Warehouse A");
        location.Active.Should().Be(StockLocationConstant.Defaults.Active);
    }

    [Fact(DisplayName = "Create: Should apply default values when not specified")]
    public void Create_WithDefaults_ShouldSetDefaultValues()
    {
        var result = StockLocationMethod.Create("Default");
        var location = result.Value;

        location.Active.Should().Be(StockLocationConstant.Defaults.Active);
        location.Default.Should().Be(StockLocationConstant.Defaults.Default);
        location.BackorderableDefault.Should().Be(StockLocationConstant.Defaults.BackorderableDefault);
        location.PropagateAllVariants.Should().Be(StockLocationConstant.Defaults.PropagateAllVariants);
        location.Position.Should().Be(StockLocationConstant.Defaults.Position);
        location.LowStockThreshold.Should().Be(StockLocationConstant.Defaults.LowStockThreshold);
        location.NotifyOnLowStock.Should().Be(StockLocationConstant.Defaults.NotifyOnLowStock);
        location.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: Should set all specified parameters")]
    public void Create_WithAllParameters_ShouldSetAllValues()
    {
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var stateId = Guid.NewGuid();

        var result = StockLocationMethod.Create(
            "Main Warehouse",
            active: false,
            isDefault: true,
            countryId: countryId,
            stateId: stateId,
            presentation: "Main",
            code: "WH-001",
            address1: "123 Main St",
            address2: "Suite 100",
            city: "New York",
            postalCode: "10001",
            phone: "+1-555-1234",
            backorderableDefault: true,
            propagateAllVariants: false,
            adminName: "Admin",
            position: 10,
            id: id);

        var location = result.Value;

        location.Id.Should().Be(id);
        location.Name.Should().Be("Main Warehouse");
        location.Active.Should().BeFalse();
        location.Default.Should().BeTrue();
        location.CountryId.Should().Be(countryId);
        location.StateId.Should().Be(stateId);
        location.Presentation.Should().Be("Main");
        location.Code.Should().Be("WH-001");
        location.Address1.Should().Be("123 Main St");
        location.Address2.Should().Be("Suite 100");
        location.City.Should().Be("New York");
        location.PostalCode.Should().Be("10001");
        location.Phone.Should().Be("+1-555-1234");
        location.BackorderableDefault.Should().BeTrue();
        location.PropagateAllVariants.Should().BeFalse();
        location.AdminName.Should().Be("Admin");
        location.Position.Should().Be(10);
    }

    [Fact(DisplayName = "Create: Should generate unique ids for different instances")]
    public void Create_WithoutExplicitId_ShouldGenerateUniqueIds()
    {
        var location1 = StockLocationMethod.Create("A").Value;
        var location2 = StockLocationMethod.Create("B").Value;

        location1.Id.Should().NotBe(location2.Id);
    }
}
