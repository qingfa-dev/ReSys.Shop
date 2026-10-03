using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Storefront.GetCartForCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCartForCheckout")]
public class GetCartForCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public GetCartForCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "projects product name, unit price, shipment total, and shipping method name")]
    public async Task Handle_ProjectsLineItemMetadata_AndShipping()
    {
        var ct = TestContext.Current.CancellationToken;
        var product = ProductMethod.Create("Classic Tee").Value;
        var variant = VariantMethod.Create(product.Id, "TEE-BLK-M").Value;
        var shippingMethod = ShippingMethodMethod.Create("Express", "flat_rate").Value;

        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ShippingMethodId = shippingMethod.Id;
        order.ShipmentTotal = 12.50m;
        order.Total = 37.50m;

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<ShippingMethod>().Add(shippingMethod);
        _dbContext.Set<Order>().Add(order);
        _dbContext.Set<LineItem>().Add(LineItemMethod.Create(order.Id, variant.Id, 2, 12.50m).Value);
        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetCartForCheckoutQueryHandler(_dbContext);
        var result = await handler.Handle(new GetCartForCheckoutQuery { CartId = order.Id }, ct);

        result.IsSuccess.Should().BeTrue();
        var line = result.Value.LineItems.Should().ContainSingle().Which;
        line.VariantId.Should().Be(variant.Id);
        line.Quantity.Should().Be(2);
        line.Name.Should().Be("Classic Tee");
        line.UnitPrice.Should().Be(12.50m);
        result.Value.ShipmentTotal.Should().Be(12.50m);
        result.Value.ShippingMethodName.Should().Be("Express");
    }

    public void Dispose() => _dbContext.Dispose();
}