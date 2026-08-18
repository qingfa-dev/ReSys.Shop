using Shared.Application.Domain.Orders;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Ordering.Features.Admin.Shared.Models;
using Module.Ordering.Features.Storefront.Shared.Mappings;

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
        var request = new OrderRequest
        {
            Currency = "USD",
        };

        var result = request.MapToDomain(userId);
        var order = result.Value;

        result.IsSuccess.Should().BeTrue();
        order.Should().NotBeNull();
        order.Currency.Should().Be(request.Currency);
        order.UserId.Should().Be(userId);
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
        response.FulfillmentState.Should().Be(order.ShipmentState);
        response.UserId.Should().Be(order.UserId);
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
        response.FulfillmentState.Should().Be(order.ShipmentState);
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

    [Fact(DisplayName = "ToDetail: Should map shipping adjustment")]
    public void ToDetail_ShouldMapShippingAdjustment()
    {
        var methodId = Guid.NewGuid();
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ReplaceShippingAdjustment(5m, methodId).IsSuccess.Should().BeTrue();

        var response = order.MapToDetail<OrderDetailResponse>();

        response.ShippingAdjustment.Should().NotBeNull();
        response.ShippingAdjustment!.Label.Should().Be("Shipping");
        response.ShippingAdjustment!.Amount.Should().Be(5m);
        response.ShippingAdjustment!.ShippingMethodId.Should().Be(methodId);
        response.ShipmentTotal.Should().Be(5m);
    }

    [Fact(DisplayName = "ToDetail: Should return null shipping adjustment when none")]
    public void ToDetail_ShippingAdjustmentNull_WhenAbsent()
    {
        var order = CreateOrder();

        var response = order.MapToDetail<OrderDetailResponse>();

        response.ShippingAdjustment.Should().BeNull();
    }

    [Fact(DisplayName = "ToDetail with lookup: Should populate line items with product fields")]
    public void ToDetail_WithLookup_ShouldPopulateLineItemsWithProducts()
    {
        var variantId = Guid.NewGuid();
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, variantId, 2, 29.99m).Value;
        order.LineItems.Add(lineItem);

        var lookup = new Dictionary<Guid, CartItemLookup>
        {
            [variantId] = new CartItemLookup
            {
                Sku = "SKU-1",
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductImageUrl = "https://example.com/image.jpg",
            }
        };

        var response = order.MapToDetailWithLookup<OrderDetailResponse>(lookup);

        var mapped = response.LineItems.Should().ContainSingle().Subject;
        mapped.OrderId.Should().Be(order.Id);
        mapped.VariantId.Should().Be(variantId);
        mapped.ProductId.Should().Be(lookup[variantId].ProductId);
        mapped.ProductName.Should().Be("Test Product");
        mapped.ProductImageUrl.Should().Be("https://example.com/image.jpg");
    }

    [Fact(DisplayName = "MapToLineItemResponse: Should populate OrderId")]
    public void MapToLineItemResponse_ShouldPopulateOrderId()
    {
        var orderId = Guid.NewGuid();
        var lineItem = LineItemMethod.Create(orderId, Guid.NewGuid(), 2, 29.99m).Value;

        var response = lineItem.MapToLineItemResponse<LineItemResponse>();

        response.OrderId.Should().Be(orderId);
        response.VariantId.Should().Be(lineItem.VariantId);
        response.Quantity.Should().Be(2);
        response.Price.Should().Be(29.99m);
    }

    [Fact(DisplayName = "MapToLineItemResponse with lookup: Should populate product fields and OrderId")]
    public void MapToLineItemResponse_WithLookup_ShouldPopulateProductFieldsAndOrderId()
    {
        var orderId = Guid.NewGuid();
        var lineItem = LineItemMethod.Create(orderId, Guid.NewGuid(), 2, 29.99m).Value;
        var lookup = new CartItemLookup
        {
            Sku = "SKU-1",
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductImageUrl = "https://example.com/image.jpg",
        };

        var response = lineItem.MapToLineItemResponse<LineItemResponse>(lookup);

        response.OrderId.Should().Be(orderId);
        response.ProductId.Should().Be(lookup.ProductId);
        response.ProductName.Should().Be(lookup.ProductName);
        response.ProductImageUrl.Should().Be(lookup.ProductImageUrl);
    }

    [Fact(DisplayName = "MapToLineItemResponse with null lookup: Should still populate OrderId without product fields")]
    public void MapToLineItemResponse_WithNullLookup_ShouldPopulateOrderIdWithoutProducts()
    {
        var orderId = Guid.NewGuid();
        var lineItem = LineItemMethod.Create(orderId, Guid.NewGuid(), 2, 29.99m).Value;

        var response = lineItem.MapToLineItemResponse<LineItemResponse>(null);

        response.OrderId.Should().Be(orderId);
        response.ProductId.Should().BeNull();
        response.ProductName.Should().BeNull();
        response.ProductImageUrl.Should().BeNull();
    }

    private static Order CreateOrder(Action<Order>? configure = null)
    {
        var order = OrderMethod.Create(
            currency: "USD",
            userId: Guid.NewGuid()).Value;
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
        order.PaymentState = OrderPaymentState.Paid;
        order.ShipmentState = ShipmentState.Pending;
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
