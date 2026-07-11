using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")][Trait("Module", "Ordering")][Trait("Entity", "Order")]
public class OrderCheckoutTests
{
    [Fact]
    public void AssignDefaultAddresses_Should_Set_Addresses_When_Null()
    {
        var storeId = Guid.NewGuid();
        var order = OrderExtensions.Create("USD", Guid.NewGuid(), storeId).Value;
        var billId = Guid.NewGuid();
        var shipId = Guid.NewGuid();

        order.AssignDefaultAddresses(billId, shipId);

        order.BillAddressId.Should().Be(billId);
        order.ShipAddressId.Should().Be(shipId);
    }

    [Fact]
    public void AssignDefaultAddresses_Should_Not_Overwrite_Existing_Addresses()
    {
        var storeId = Guid.NewGuid();
        var existingBillId = Guid.NewGuid();
        var existingShipId = Guid.NewGuid();
        var order = OrderExtensions.Create("USD", Guid.NewGuid(), storeId, shipAddressId: existingShipId).Value;
        order.BillAddressId = existingBillId;

        var newBillId = Guid.NewGuid();
        var newShipId = Guid.NewGuid();

        order.AssignDefaultAddresses(newBillId, newShipId);

        order.BillAddressId.Should().Be(existingBillId);
        order.ShipAddressId.Should().Be(existingShipId);
    }

    [Fact]
    public void AssignDefaultAddresses_Should_Not_Set_When_Null_Provided()
    {
        var storeId = Guid.NewGuid();
        var order = OrderExtensions.Create("USD", Guid.NewGuid(), storeId).Value;

        order.AssignDefaultAddresses(null, null);

        order.BillAddressId.Should().BeNull();
        order.ShipAddressId.Should().BeNull();
    }
}
