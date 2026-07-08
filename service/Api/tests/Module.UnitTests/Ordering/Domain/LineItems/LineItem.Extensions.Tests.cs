using Module.Ordering.Domain.LineItems;
namespace Module.UnitTests.Ordering.Domain.LineItems;
[Trait("Category","Unit")][Trait("Module","Ordering")][Trait("Entity","LineItem")]
public class LineItemExtensionsTests
{
    [Fact]public void Create_WithValidParams_ShouldReturnLineItem(){
        var r=LineItemExtensions.Create(Guid.NewGuid(),Guid.NewGuid(),2,19.99m);
        r.IsSuccess.Should().BeTrue();r.Value.Quantity.Should().Be(2);r.Value.Price.Should().Be(19.99m);
    }
    [Fact]public void Create_WithZeroQuantity_ShouldFail(){
        var r=LineItemExtensions.Create(Guid.NewGuid(),Guid.NewGuid(),0,10);r.IsFailure.Should().BeTrue();
    }
    [Fact]public void Create_WithNegativePrice_ShouldFail(){
        var r=LineItemExtensions.Create(Guid.NewGuid(),Guid.NewGuid(),1,-5);r.IsFailure.Should().BeTrue();
    }
    [Fact]public void UpdateQuantity_ShouldRecalculate(){
        var li=LineItemExtensions.Create(Guid.NewGuid(),Guid.NewGuid(),1,10).Value;
        var r=li.UpdateQuantity(3);r.IsSuccess.Should().BeTrue();li.Quantity.Should().Be(3);
    }

}
