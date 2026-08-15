using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CartMapping")]
public class CartMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should return default values (stub)")]
    public void MapToDetail_ShouldReturnDefaultValues()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid(), Guid.Empty).Value;

        var response = cart.MapToDetail<CartDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(cart.Id);
    }

    [Fact(DisplayName = "MapToDetail: Should map all properties from Order entity")]
    public void MapToDetail_ShouldMapAllProperties()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid(), Guid.Empty).Value;
        cart.ItemTotal = 100m;
        cart.Total = 120m;
        cart.ItemCount = 3;
        cart.CheckoutState = CheckoutState.PickDeliveryMethod;

        var response = cart.MapToDetail<CartDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(cart.Id);
        response.ItemTotal.Should().Be(100m);
        response.Total.Should().Be(120m);
        response.Currency.Should().Be("USD");
        response.ItemCount.Should().Be(3);
        response.CheckoutState.Should().Be(CheckoutState.PickDeliveryMethod);
    }

    [Fact(DisplayName = "MapToDetail: Should map the shipping breakdown")]
    public void MapToDetail_ShouldMapShippingBreakdown()
    {
        var methodId = Guid.NewGuid();
        var cart = OrderMethod.Create("USD", Guid.NewGuid(), Guid.Empty).Value;
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(), OrderId = cart.Id, VariantId = Guid.NewGuid(),
            Quantity = 1, Price = 100m, Total = 100m, Currency = "USD"
        });
        cart.SetShippingMethod(methodId);
        cart.ReplaceShippingAdjustment(5m, methodId).IsSuccess.Should().BeTrue();
        cart.ShipmentTotal = 5m;
        cart.AdjustmentTotal = 2m;
        cart.Total = 107m;

        var response = cart.MapToDetail<CartDetailResponse>();

        response.ShipmentTotal.Should().Be(cart.ShipmentTotal);
        response.AdjustmentTotal.Should().Be(cart.AdjustmentTotal);
        response.ShippingAdjustment.Should().NotBeNull();
        response.ShippingAdjustment!.Label.Should().Be("Shipping");
        response.ShippingAdjustment!.Amount.Should().Be(5m);
        response.ShippingAdjustment!.ShippingMethodId.Should().Be(methodId);
        response.Total.Should().Be(response.ItemTotal + response.ShipmentTotal + response.AdjustmentTotal);
    }

    [Fact(DisplayName = "MapToDetail: Should return null shipping adjustment when cart has none")]
    public void MapToDetail_ShippingAdjustmentNull_WhenNoShippingAdjustment()
    {
        var cart = OrderMethod.Create("USD", Guid.NewGuid(), Guid.Empty).Value;

        var response = cart.MapToDetail<CartDetailResponse>();

        response.ShippingAdjustment.Should().BeNull();
    }
}
