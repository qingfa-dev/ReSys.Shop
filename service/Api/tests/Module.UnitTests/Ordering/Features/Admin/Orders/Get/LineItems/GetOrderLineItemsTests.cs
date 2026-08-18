using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.LineItems;
using Shared.Operational.Persistence.Specifications.Querying;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.LineItems;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderLineItems")]
public class GetOrderLineItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderLineItems.PagedQueryHandler _handler;

    public GetOrderLineItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderLineItems.PagedQueryHandler(_dbContext);
    }

    [Fact(DisplayName = "Handler: Should enrich line items with product fields and OrderId")]
    public async Task Handle_ShouldEnrichLineItemsWithProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;

        var product = ProductMethod.Create("Test Product").Value;
        var variant = VariantMethod.Create(product.Id, "SKU-1", isMaster: true).Value;
        var image = VariantImageMethod.Create("image/jpeg", "img.jpg", 100, "https://example.com/image.jpg", "path/img.jpg", position: 0, variantId: variant.Id).Value;
        variant.VariantImages.Add(image);
        product.Variants.Add(variant);
        product.MasterVariantId = variant.Id;

        var lineItem = LineItemMethod.Create(order.Id, variant.Id, 2, 29.99m).Value;

        _dbContext.Set<Order>().Add(order);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<VariantImage>().Add(image);
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetOrderLineItems.Query(order.Id, QueryingParameters.Empty),
            ct);

        result.Items.Should().ContainSingle();
        var item = result.Items.Single();
        item.OrderId.Should().Be(order.Id);
        item.Id.Should().Be(lineItem.Id);
        item.VariantId.Should().Be(variant.Id);
        item.ProductId.Should().Be(product.Id);
        item.ProductName.Should().Be("Test Product");
        item.ProductImageUrl.Should().Be("https://example.com/image.jpg");
    }

    public void Dispose() => _dbContext.Dispose();
}
