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
}
