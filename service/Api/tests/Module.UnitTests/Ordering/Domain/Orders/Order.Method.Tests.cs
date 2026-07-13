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
        order.RecalculateTotals();
        order.AdjustmentTotal.Should().Be(7m); // line item adj (2) + order adj (5)
        order.ItemTotal.Should().Be(10m);
        order.Total.Should().Be(17m);
        order.OutstandingBalance.Should().Be(17m);
    }
}
