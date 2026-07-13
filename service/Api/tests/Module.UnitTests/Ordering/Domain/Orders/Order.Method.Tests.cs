using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")][Trait("Module", "Ordering")][Trait("Entity", "Order")]
public class OrderMethodTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnOrder()
    {
        var storeId = Guid.NewGuid();
        var result = OrderMethod.Create("USD", Guid.NewGuid(), storeId);
        var order = result.Value;
        result.IsSuccess.Should().BeTrue();
        order.Currency.Should().Be("USD");
        order.Status.Should().Be(OrderStatus.Draft);
        order.StoreId.Should().Be(storeId);
        order.ItemTotal.Should().Be(0);
        order.Total.Should().Be(0);
    }

    [Fact]
    public void Finalize_WithItems_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Finalize();
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Placed);
        order.CompletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Finalize_WhenCanceled_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Canceled;
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AlreadyCanceled);
    }

    [Fact]
    public void Finalize_AlreadyPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AlreadyFinalized);
    }

    [Fact]
    public void Finalize_EmptyOrder_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Finalize();
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.EmptyOrderCannotFinalize);
    }

    [Fact]
    public void Cancel_WhenPlaced_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Cancel(Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Cancel(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCanceled_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.Status = OrderStatus.Canceled;
        var r = order.Cancel(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AlreadyCanceled);
    }

    [Fact]
    public void Empty_ShouldClearItemsAndTotals()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.ItemTotal = 10;
        order.Total = 10;
        var r = order.Empty();
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().BeEmpty();
        order.Total.Should().Be(0);
    }

    [Fact]
    public void Delete_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Delete("test-user");
        r.IsSuccess.Should().BeTrue();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Delete("test-user");
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.InvalidStatusForDelete);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.ApprovedById = Guid.NewGuid();
        var r = order.Approve(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AlreadyApproved);
    }

    [Fact]
    public void Empty_ShouldClearItemCount()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 3, Price = 10 });
        order.ItemCount = 3;
        var r = order.Empty();
        r.IsSuccess.Should().BeTrue();
        order.ItemCount.Should().Be(0);
    }

    [Fact]
    public void RecalculateTotals_ShouldIncludeLineItemAdjustments()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10, Total = 10, AdjustmentTotal = 2 });
        order.Adjustments.Add(new()
        {
            Amount = 5,
            Eligible = true,
            Label = "Tax",
            DisplayAmount = "5.00",
            AdjustableId = order.Id,
            AdjustableType = AdjustmentConstant.AdjustableTypes.Order,
            SourceId = Guid.NewGuid(),
            SourceType = "Tax",
            OrderId = order.Id,
            CreatedBy = "test"
        });
        var result = order.RecalculateTotals();
        result.IsSuccess.Should().BeTrue();
        order.AdjustmentTotal.Should().Be(7m); // line item adj (2) + order adj (5)
        order.ItemTotal.Should().Be(10m);
        order.Total.Should().Be(17m);
        order.OutstandingBalance.Should().Be(17m);
    }

    [Fact]
    public void Place_WithValidPrerequisites_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Confirm;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Place("R20260713-1A2B3C4D");
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Placed);
        order.CheckoutState.Should().Be(CheckoutState.Complete);
        order.Number.Should().Be("R20260713-1A2B3C4D");
    }

    [Fact]
    public void Place_MissingAddresses_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Confirm;
        order.ShippingMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Place("R-test");
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AddressRequired);
    }

    [Fact]
    public void Complete_WhenPlaced_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Complete("tester");
        r.IsSuccess.Should().BeTrue();
        order.CheckoutState.Should().Be(CheckoutState.Complete);
    }

    [Fact]
    public void Complete_WhenDraft_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Complete("tester");
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.UpdateDetails("a@b.com", "Handle with care", null, null, null);
        r.IsSuccess.Should().BeTrue();
        order.Email.Should().Be("a@b.com");
        order.SpecialInstructions.Should().Be("Handle with care");
    }

    [Fact]
    public void UpdateDetails_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.UpdateDetails("a@b.com", null, null, null, null);
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetBillAddress_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var id = Guid.NewGuid();
        var r = order.SetBillAddress(id);
        r.IsSuccess.Should().BeTrue();
        order.BillAddressId.Should().Be(id);
    }

    [Fact]
    public void SetShipAddress_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.SetShipAddress(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetShippingMethod_ShouldResetShipmentTotal()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.ShipmentTotal = 99m;
        var r = order.SetShippingMethod(Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        order.ShipmentTotal.Should().Be(0m);
    }

    [Fact]
    public void AddLineItem_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 2, 15m).Value;
        var r = order.AddLineItem(lineItem);
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().Contain(lineItem);
    }

    [Fact]
    public void RemoveLineItem_ShouldRemoveAndRecalculate()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 1, 10m).Value;
        order.LineItems.Add(lineItem);
        order.ItemTotal = 10m;
        var r = order.RemoveLineItem(lineItem.Id);
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLineItem_WhenNotFound_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.RemoveLineItem(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TransferOwnership_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var userId = Guid.NewGuid();
        var r = order.TransferOwnership(userId);
        r.IsSuccess.Should().BeTrue();
        order.UserId.Should().Be(userId);
        order.SessionId.Should().BeNull();
    }

    [Fact]
    public void HasAddresses_WhenBothSet_ShouldReturnTrue()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.HasAddresses().Should().BeTrue();
    }

    [Fact]
    public void HasAddresses_WhenMissing_ShouldReturnFalse()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.BillAddressId = Guid.NewGuid();
        order.HasAddresses().Should().BeFalse();
    }

    [Fact]
    public void CanModifyLineItems_WhenDraft_ShouldReturnTrue()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CanModifyLineItems().Should().BeTrue();
    }

    [Fact]
    public void CanModifyLineItems_WhenPlaced_ShouldReturnFalse()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        order.CanModifyLineItems().Should().BeFalse();
    }
}
