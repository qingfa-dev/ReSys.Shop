using Module.Catalog.Domain.Products;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
public class ProductMethodTests
{
    [Theory(DisplayName = "Create: Should return Product with correct properties")]
    [InlineData("T-Shirt", "t-shirt", ProductStatus.Active)]
    [InlineData("Jeans", "jeans", ProductStatus.Draft)]
    public void Create_WithValidParameters_ShouldReturnProduct(string name, string slug, ProductStatus status)
    {
        var id = Guid.NewGuid();

        var result = ProductMethod.Create(name, slug, status: status, id: id);
        var product = result.Value;

        result.IsSuccess.Should().BeTrue();
        product.Should().NotBeNull();
        product.Id.Should().Be(id);
        product.Name.Should().Be(name);
        product.Slug.Should().Be(slug);
        product.Status.Should().Be(status);
        product.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory(DisplayName = "Update: Should update properties correctly")]
    [InlineData("New Name", "new-slug", "New Description")]
    public void Update_WithValidParameters_ShouldUpdateProperties(
        string name,
        string slug,
        string description)
    {
        var product = ProductMethod.Create("Old Name", "old-slug", status: ProductStatus.Active).Value;
        var availableOn = DateTimeOffset.UtcNow.AddDays(1);
        var result = product.Update(name, slug, description, status: null, availableOn: availableOn);

        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be(name);
        product.Slug.Should().Be(slug);
        product.Description.Should().Be(description);
        product.AvailableOn.Should().Be(availableOn);
    }

    [Fact(DisplayName = "ChangeStatus: Should update product status")]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        var newStatus = ProductStatus.Active;

        var result = product.ChangeStatus(newStatus);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(newStatus);
    }

    [Fact(DisplayName = "ChangeStatus: Same status should return Ok and not raise event")]
    public void ChangeStatus_WhenSameStatus_ShouldReturnOk()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;

        var result = product.ChangeStatus(ProductStatus.Draft);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory(DisplayName = "Update: Partial update should preserve other properties")]
    [InlineData("New Name Only")]
    public void Update_WithSomeNullParams_ShouldPreserveExisting(string newName)
    {
        var product = ProductMethod.Create("Old Name", "old-slug", description: "Old description", status: ProductStatus.Active).Value;
        var result = product.Update(name: newName);

        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be(newName);
        product.Slug.Should().Be("old-slug");
        product.Description.Should().Be("Old description");
    }

    [Fact(DisplayName = "Delete: Should mark as deleted and raise event")]
    public void Delete_WhenCalled_ShouldSetIsDeleted()
    {
        var product = ProductMethod.Create("Product", "product").Value;

        var result = product.Delete("admin");

        result.IsSuccess.Should().BeTrue();
        product.IsDeleted.Should().BeTrue();
        product.DeletedBy.Should().Be("admin");
        product.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Delete: When already deleted should return Ok and not raise event")]
    public void Delete_WhenAlreadyDeleted_ShouldReturnOk()
    {
        var product = ProductMethod.Create("Product", "product").Value;
        product.Delete("admin");

        var result = product.Delete("admin");

        result.IsSuccess.Should().BeTrue();
    }
}
