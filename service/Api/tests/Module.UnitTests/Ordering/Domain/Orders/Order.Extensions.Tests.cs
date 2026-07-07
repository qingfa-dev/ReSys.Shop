using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")][Trait("Module", "Ordering")][Trait("Entity", "Order")]
public class OrderExtensionsTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnOrder()
    {
        var storeId = Guid.NewGuid();
        var result = OrderExtensions.Create("USD", Guid.NewGuid(), storeId);
        var order = result.Value;
        result.IsSuccess.Should().BeTrue();
        order.Currency.Should().Be("USD");
        order.Status.Should().Be(OrderStatus.Draft);
        order.StoreId.Should().Be(storeId);
        order.ItemTotal.Should().Be(0);
        order.Total.Should().Be(0);
    }

    [Fact]
    public void AdvanceCheckout_FromAddress_ShouldTransition()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Address;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        var r = order.AdvanceCheckout();
        r.IsSuccess.Should().BeTrue();
        order.CheckoutState.Should().Be(CheckoutState.Delivery);
    }

    [Fact]
    public void AdvanceCheckout_WithoutAddress_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Address;
        var r = order.AdvanceCheckout();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.AddressRequired);
    }

    [Fact]
    public void AdvanceCheckout_DeliveryWithoutMethod_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Delivery;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        var r = order.AdvanceCheckout();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.DeliveryMethodRequired);
    }

    [Fact]
    public void AdvanceCheckout_DeliveryWithMethod_ShouldTransition()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Delivery;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        var r = order.AdvanceCheckout();
        r.IsSuccess.Should().BeTrue();
        order.CheckoutState.Should().Be(CheckoutState.Payment);
    }

    [Fact]
    public void AdvanceCheckout_FromComplete_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Complete;
        var r = order.AdvanceCheckout();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.CannotAdvanceState);
    }

    [Fact]
    public void Finalize_WithItems_ShouldSucceed()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Finalize();
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Placed);
        order.CompletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Finalize_WhenCanceled_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Canceled;
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.AlreadyCanceled);
    }

    [Fact]
    public void Finalize_AlreadyPlaced_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.AlreadyFinalized);
    }

    [Fact]
    public void Finalize_EmptyOrder_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.EmptyOrderCannotFinalize);
    }

    [Fact]
    public void Cancel_WhenPlaced_ShouldSucceed()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Cancel(Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Cancel(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCanceled_ShouldFail()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Canceled;
        var r = order.Cancel(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
        r.FirstFailure.Should().Be(OrderResult.Errors.AlreadyCanceled);
    }

    [Fact]
    public void Empty_ShouldClearItemsAndTotals()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.ItemTotal = 10;
        order.Total = 10;
        var r = order.Empty();
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().BeEmpty();
        order.Total.Should().Be(0);
    }

    [Fact]
    public void IsPaid_WhenBalanceZero_ShouldReturnTrue()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.OutstandingBalance = 0;
        order.IsPaid().Should().BeTrue();
    }

    [Fact]
    public void IsPaid_WhenBalancePositive_ShouldReturnFalse()
    {
        var order = OrderExtensions.Create("USD", null, Guid.NewGuid()).Value;
        order.OutstandingBalance = 50;
        order.IsPaid().Should().BeFalse();
    }
}
