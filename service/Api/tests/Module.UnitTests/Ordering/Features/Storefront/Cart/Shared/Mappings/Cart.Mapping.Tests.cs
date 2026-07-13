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

    [Fact(
        DisplayName = "MapToDetail: Enrich with real property mapping",
        Skip = "CartMapping.MapToDetail<T> is a stub returning defaults. Enrich mapping first.")]
    public void MapToDetail_ShouldMapAllProperties()
    {
    }
}
