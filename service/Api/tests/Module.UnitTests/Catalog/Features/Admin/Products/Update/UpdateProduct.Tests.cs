using MediatR;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Update;
using Module.Catalog.Features.Admin.Products.Variants.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductUpdate")]
public class UpdateProductTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ILogger<UpdateProduct.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateProduct.CommandHandler _handler;

    public UpdateProductTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<UpdateVariant.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateVariant.Response>.Ok(new UpdateVariant.Response()));

        _loggerMock = new Mock<ILogger<UpdateProduct.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UpdateProduct.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update product and dispatch UpdateVariant for master variant")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = ProductMethod.Create("Original", "original", status: ProductStatus.Draft).Value;
        var masterVariant = VariantExtensions.Create(product.Id, "original-master", isMaster: true).Value;
        product.Variants.Add(masterVariant);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        product.MasterVariantId = masterVariant.Id;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateProduct.Request
        {
            Name = "Updated",
            Slug = "updated",
            Description = "Updated description",
            Price = 29.99m,
        };

        var result = await _handler.Handle(new UpdateProduct.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
        result.Value.Slug.Should().Be("updated");

        var persisted = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Updated");

        _senderMock.Verify(x => x.Send(
            It.Is<UpdateVariant.Command>(c => c.Id == masterVariant.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateProduct.Request { Name = "Test", Slug = "test" };

        var result = await _handler.Handle(new UpdateProduct.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);

        _senderMock.Verify(x => x.Send(It.IsAny<UpdateVariant.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure when slug conflicts with another product")]
    public async Task Handle_ShouldReturnFailure_WhenSlugIsDuplicate()
    {
        var product = ProductMethod.Create("Original", "original", status: ProductStatus.Draft).Value;
        var masterVariant = VariantExtensions.Create(product.Id, "original-master", isMaster: true).Value;
        product.Variants.Add(masterVariant);
        _dbContext.Set<Product>().Add(product);

        var other = ProductMethod.Create("Other", "existing-slug", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(other);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        product.MasterVariantId = masterVariant.Id;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateProduct.Request { Name = "Updated", Slug = "existing-slug" };

        var result = await _handler.Handle(new UpdateProduct.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.DuplicateSlug.Code);

        _senderMock.Verify(x => x.Send(It.IsAny<UpdateVariant.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should dispatch UpdateVariant with variant fields")]
    public async Task Handle_ShouldUpdateMasterVariant_WhenVariantFieldsProvided()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        var masterVariant = VariantExtensions.Create(product.Id, "product-master", isMaster: true).Value;
        product.Variants.Add(masterVariant);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        product.MasterVariantId = masterVariant.Id;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateProduct.Request
        {
            Name = "Updated",
            Slug = "updated",
            Price = 49.99m,
            CostPrice = 20m,
            CostCurrency = "USD",
            Weight = 2.5m,
            WeightUnit = "kg",
        };

        await _handler.Handle(new UpdateProduct.Command(product.Id, request), TestContext.Current.CancellationToken);

        _senderMock.Verify(x => x.Send(
            It.Is<UpdateVariant.Command>(c =>
                c.Id == masterVariant.Id &&
                c.Request.Price == 49.99m &&
                c.Request.CostPrice == 20m &&
                c.Request.CostCurrency == "USD" &&
                c.Request.Weight == 2.5m &&
                c.Request.WeightUnit == "kg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should not dispatch UpdateVariant when no master variant exists")]
    public async Task Handle_ShouldNotDispatchUpdateVariant_WhenNoMasterVariant()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateProduct.Request { Name = "Updated", Slug = "updated" };

        var result = await _handler.Handle(new UpdateProduct.Command(product.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _senderMock.Verify(x => x.Send(It.IsAny<UpdateVariant.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
