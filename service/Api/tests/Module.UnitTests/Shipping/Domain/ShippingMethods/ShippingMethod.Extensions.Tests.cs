using Module.Shipping.Domain.ShippingMethods;
namespace Module.UnitTests.Shipping.Domain.ShippingMethods;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","ShippingMethod")]
public class ShippingMethodExtensionsTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnMethod()
    {
        var r = ShippingMethodExtensions.Create("Standard", "FlatRate");
        r.IsSuccess.Should().BeTrue();
        r.Value.Name.Should().Be("Standard");
        r.Value.CalculatorType.Should().Be("FlatRate");
        r.Value.AvailableToUsers.Should().BeTrue();
    }
    [Fact]
    public void Update_WithNewValues_ShouldUpdate()
    {
        var m = ShippingMethodExtensions.Create("Old", "FlatRate").Value;
        var r = m.Update("New", "NEWCODE", "https://track.example.com/:tracking", "Admin", 2, false, "PerItem", Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        m.Name.Should().Be("New");
        m.Code.Should().Be("NEWCODE");
        m.TrackingUrl.Should().Be("https://track.example.com/:tracking");
        m.AdminName.Should().Be("Admin");
        m.Position.Should().Be(2);
        m.AvailableToUsers.Should().BeFalse();
        m.CalculatorType.Should().Be("PerItem");
    }
    [Fact]
    public void BuildTrackingUrl_WithTemplate_ShouldSubstitute()
    {
        var m = ShippingMethodExtensions.Create("Std", "FlatRate").Value;
        m.TrackingUrl = "https://track.example.com/:tracking";
        m.BuildTrackingUrl("ABC123").Should().Be("https://track.example.com/ABC123");
    }
    [Fact]
    public void BuildTrackingUrl_WithoutTemplate_ShouldReturnEmpty()
    {
        var m = ShippingMethodExtensions.Create("Std", "FlatRate").Value;
        m.BuildTrackingUrl("ABC123").Should().Be(string.Empty);
    }
    [Fact]
    public void IsAvailableFor_NoArgs_WhenAvailable_ShouldReturnTrue()
    {
        var m = ShippingMethodExtensions.Create("Std", "FlatRate").Value;
        m.AvailableToUsers = true;
        m.IsAvailableFor().Should().BeTrue();
    }
    [Fact]
    public void IsAvailableFor_NoArgs_WhenDeleted_ShouldReturnFalse()
    {
        var m = ShippingMethodExtensions.Create("Std", "FlatRate").Value;
        m.IsDeleted = true;
        m.IsAvailableFor().Should().BeFalse();
    }
}
