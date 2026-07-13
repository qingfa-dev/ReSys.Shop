using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderMapping")]
public class OrderMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var request = new OrderRequest
        {
            Currency = "USD",
        };

        var result = request.MapToDomain(userId, storeId);
        var order = result.Value;

        result.IsSuccess.Should().BeTrue();
        order.Should().NotBeNull();
        order.Currency.Should().Be(request.Currency);
        order.UserId.Should().Be(userId);
        order.StoreId.Should().Be(storeId);
        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var order = CreateOrder();

        var response = order.MapToDetail<OrderDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(order.Id);
        response.Number.Should().Be(order.Number);
        response.Status.Should().Be(order.Status);
        response.CheckoutState.Should().Be(order.CheckoutState);
        response.Currency.Should().Be(order.Currency);
        response.Email.Should().Be(order.Email);
        response.SpecialInstructions.Should().Be(order.SpecialInstructions);
        response.BillAddressId.Should().Be(order.BillAddressId);
        response.ShipAddressId.Should().Be(order.ShipAddressId);
        response.ShippingMethodId.Should().Be(order.ShippingMethodId);
        response.ItemTotal.Should().Be(order.ItemTotal);
        response.AdjustmentTotal.Should().Be(order.AdjustmentTotal);
        response.ShipmentTotal.Should().Be(order.ShipmentTotal);
        response.Total.Should().Be(order.Total);
        response.PaymentTotal.Should().Be(order.PaymentTotal);
        response.OutstandingBalance.Should().Be(order.OutstandingBalance);
        response.PaymentState.Should().Be(order.PaymentState);
        response.ShipmentState.Should().Be(order.ShipmentState);
        response.UserId.Should().Be(order.UserId);
        response.StoreId.Should().Be(order.StoreId);
        response.ItemCount.Should().Be(order.ItemCount);
        response.ApprovedById.Should().Be(order.ApprovedById);
        response.ApprovedAtUtc.Should().Be(order.ApprovedAtUtc);
        response.CompletedAtUtc.Should().Be(order.CompletedAtUtc);
        response.CanceledAtUtc.Should().Be(order.CanceledAtUtc);
        response.CreatedAtUtc.Should().Be(order.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(order.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should handle null timestamps")]
    public void ToDetail_WhenTimestampsAreNull_ShouldMapCorrectly()
    {
        var order = CreateOrder(o =>
        {
            o.CompletedAtUtc = null;
            o.CanceledAtUtc = null;
            o.ModifiedAtUtc = null;
            o.ApprovedAtUtc = null;
            o.ApprovedById = null;
        });

        var response = order.MapToDetail<OrderDetailResponse>();

        response.CompletedAtUtc.Should().BeNull();
        response.CanceledAtUtc.Should().BeNull();
        response.ModifiedAtUtc.Should().BeNull();
        response.ApprovedAtUtc.Should().BeNull();
        response.ApprovedById.Should().BeNull();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var order = CreateOrder();

        var response = order.MapToListItem<OrderListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(order.Id);
        response.Number.Should().Be(order.Number);
        response.Status.Should().Be(order.Status);
        response.Currency.Should().Be(order.Currency);
        response.Total.Should().Be(order.Total);
        response.PaymentTotal.Should().Be(order.PaymentTotal);
        response.PaymentState.Should().Be(order.PaymentState);
        response.ShipmentState.Should().Be(order.ShipmentState);
        response.BillAddressId.Should().Be(order.BillAddressId);
        response.ShipAddressId.Should().Be(order.ShipAddressId);
        response.Email.Should().Be(order.Email);
        response.CreatedAtUtc.Should().Be(order.CreatedAtUtc);
        response.CompletedAtUtc.Should().Be(order.CompletedAtUtc);
    }

    [Fact(DisplayName = "ToListItem: Should handle null email and optional fields")]
    public void ToListItem_WhenOptionalFieldsAreNull_ShouldMapCorrectly()
    {
        var order = CreateOrder(o =>
        {
            o.Email = null;
            o.CompletedAtUtc = null;
            o.BillAddressId = null;
            o.ShipAddressId = null;
        });

        var response = order.MapToListItem<OrderListItemResponse>();

        response.Email.Should().BeNull();
        response.CompletedAtUtc.Should().BeNull();
        response.BillAddressId.Should().BeNull();
        response.ShipAddressId.Should().BeNull();
    }

    private static Order CreateOrder(Action<Order>? configure = null)
    {
        var order = OrderMethod.Create(
            currency: "USD",
            userId: Guid.NewGuid(),
            storeId: Guid.NewGuid()).Value;
        order.Email = "test@example.com";
        order.SpecialInstructions = "Leave at door";
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        order.ItemTotal = 50m;
        order.AdjustmentTotal = 5m;
        order.ShipmentTotal = 15m;
        order.Total = 80m;
        order.PaymentTotal = 80m;
        order.OutstandingBalance = 0m;
        order.PaymentState = "paid";
        order.ShipmentState = "pending";
        order.Status = OrderStatus.Placed;
        order.CheckoutState = CheckoutState.Complete;
        order.ItemCount = 5;
        order.ApprovedById = Guid.NewGuid();
        order.ApprovedAtUtc = DateTimeOffset.UtcNow;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        configure?.Invoke(order);
        return order;
    }
}
