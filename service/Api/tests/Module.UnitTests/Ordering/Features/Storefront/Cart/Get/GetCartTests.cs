using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Get;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCart")]
public class GetCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly GetCart.QueryHandler _handler;

    public GetCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(Variant).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);

        _handler = new GetCart.QueryHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should enrich cart items with product name and primary image")]
    public async Task Handle_ShouldReturnProductNameAndImage_WhenCartHasItems()
    {
        // Arrange: Seed product with a master variant carrying a primary image
        var product = ProductMethod.Create("Blue T-Shirt", status: ProductStatus.Active).Value;
        var masterVariant = VariantMethod.Create(product.Id, "TSHIRT-M", isMaster: true).Value;
        var image = VariantImageMethod.Create(
            "image/jpeg", "tshirt.jpg", 1024, "https://img.test/tshirt.jpg", "/storage/tshirt.jpg",
            position: 0, variantId: masterVariant.Id).Value;
        masterVariant.VariantImages.Add(image);
        product.Variants.Add(masterVariant);
        product.MasterVariantId = masterVariant.Id;
        _dbContext.Set<Product>().Add(product);

        // Arrange: Seed a draft cart with one line item referencing the master variant
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = masterVariant.Id,
            Quantity = 2,
            Price = 19.99m,
            Total = 39.98m,
            Currency = "USD",
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetCart.Query(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.ProductName.Should().Be("Blue T-Shirt");
        item.ProductImageUrl.Should().Be("https://img.test/tshirt.jpg");
        item.Sku.Should().Be("TSHIRT-M");
        item.VariantName.Should().Be("TSHIRT-M");
    }

    [Fact(DisplayName = "Handler: Should fall back to another variant's image when the master variant has none")]
    public async Task Handle_ShouldFallBackToVariantImage_WhenMasterHasNoImage()
    {
        // Arrange: Seed product whose master variant has no images, but a regular variant does
        var product = ProductMethod.Create("Canvas Shoes", status: ProductStatus.Active).Value;
        var masterVariant = VariantMethod.Create(product.Id, "SHOES-M", isMaster: true).Value;
        var regularVariant = VariantMethod.Create(product.Id, "SHOES-09", isMaster: false).Value;
        var image = VariantImageMethod.Create(
            "image/jpeg", "shoes.jpg", 2048, "https://img.test/shoes.jpg", "/storage/shoes.jpg",
            position: 0, variantId: regularVariant.Id).Value;
        regularVariant.VariantImages.Add(image);
        product.Variants.Add(masterVariant);
        product.Variants.Add(regularVariant);
        product.MasterVariantId = masterVariant.Id;
        _dbContext.Set<Product>().Add(product);

        // Arrange: Seed a draft cart with a line item referencing the master variant
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = masterVariant.Id,
            Quantity = 1,
            Price = 29.99m,
            Total = 29.99m,
            Currency = "USD",
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetCart.Query(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.ProductName.Should().Be("Canvas Shoes");
        item.ProductImageUrl.Should().Be("https://img.test/shoes.jpg");
    }

    [Fact(DisplayName = "Handler: Should leave image null when the product has no images")]
    public async Task Handle_ShouldReturnNullImage_WhenProductHasNoImages()
    {
        // Arrange: Seed a product with a master variant but no images
        var product = ProductMethod.Create("Plain Mug", status: ProductStatus.Active).Value;
        var masterVariant = VariantMethod.Create(product.Id, "MUG-M", isMaster: true).Value;
        product.Variants.Add(masterVariant);
        product.MasterVariantId = masterVariant.Id;
        _dbContext.Set<Product>().Add(product);

        // Arrange: Seed a draft cart with a line item referencing the master variant
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderMethod.Create("USD", userId, Guid.Empty).Value;
        cart.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = masterVariant.Id,
            Quantity = 1,
            Price = 9.99m,
            Total = 9.99m,
            Currency = "USD",
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetCart.Query(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.ProductName.Should().Be("Plain Mug");
        item.ProductImageUrl.Should().BeNull();
    }
}
