using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Get.ById;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductGetById")]
public class GetProductByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductById.QueryHandler _handler;

    public GetProductByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return product with all relations")]
    public async Task Handle_ShouldReturnSuccess_WhenProductExists()
    {
        var product = ProductMethod.Create("Product", "product", description: "Desc", status: ProductStatus.Active).Value;
        var variant = VariantMethod.Create(product.Id, "SKU-001", isMaster: true).Value;
        product.Variants.Add(variant);
        product.MasterVariantId = variant.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductById.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Product");
        result.Value.Slug.Should().Be("product");
        result.Value.Description.Should().Be("Desc");
        result.Value.Status.Should().Be(ProductStatus.Active);
        result.Value.MasterVariantId.Should().Be(variant.Id);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new GetProductById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(Guid.Empty).Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when product is soft-deleted")]
    public async Task Handle_ShouldReturnNotFound_WhenProductIsSoftDeleted()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        product.Delete("admin");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductById.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ProductResult.Errors.NotFound(product.Id).Code);
    }
}
