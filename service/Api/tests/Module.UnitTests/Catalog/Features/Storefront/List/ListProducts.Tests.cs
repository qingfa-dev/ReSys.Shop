using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Options;
using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Get.PagedOrAll;
using Module.Inventory.Services;

namespace Module.UnitTests.Catalog.Features.Storefront.List;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontListProducts")]
public class ListProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStockAvailabilityCalculator> _calculatorMock;
    private readonly GetStorefrontProducts.PagedQueryHandler _handler;

    public ListProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _calculatorMock = new Mock<IStockAvailabilityCalculator>();
        _calculatorMock
            .Setup(c => c.GetAvailableByVariantAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
        _calculatorMock
            .Setup(c => c.GetBackorderableByVariantAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());
        _handler = new GetStorefrontProducts.PagedQueryHandler(_dbContext, _calculatorMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Product CreateActiveProduct(string name, string slug, DateTimeOffset? availableOn = null)
    {
        var product = ProductMethod.Create(name, slug, status: ProductStatus.Active).Value;
        product.AvailableOn = availableOn ?? DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        return product;
    }

    [Fact(DisplayName = "Handler: Should return all active products with empty parameters")]
    public async Task Handle_ShouldReturnAllActiveProducts_WhenNoFilters()
    {
        var product = CreateActiveProduct("Blue T-Shirt", "blue-tshirt");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Blue T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should map nested variant details in list response")]
    public async Task Handle_ShouldMapNestedVariantDetails()
    {
        var product = CreateActiveProduct("Red T-Shirt", "red-tshirt");
        var variant = VariantMethod.Create(product.Id, "RED", isMaster: true).Value;
        variant.Prices.Add(PriceMethod.Create(29.99m, "USD", variant.Id).Value);
        product.Variants.Add(variant);
        product.MasterVariantId = variant.Id;

        var optionType = OptionTypeMethod.Create("Color", "Color", filterable: true).Value;
        var optionValue = OptionValueMethod.Create(optionType.Id, "Red", "Red").Value;
        optionType.OptionValues.Add(optionValue);
        variant.OptionValueVariants.Add(OptionValueVariantMethod.Create(variant.Id, optionValue.Id).Value);

        var image = VariantImageMethod.Create(
            "image/jpeg", "img.jpg", 1024,
            "https://example.com/img.jpg", "/storage/img.jpg",
            position: 1, alt: "Red shirt", type: VariantImageType.Default).Value;
        image.VariantId = variant.Id;
        variant.VariantImages.Add(image);

        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var item = result.Items.First();
        item.MasterVariant.Should().NotBeNull();
        item.MasterVariant!.Price.Should().Be(29.99m);
        item.MasterVariant!.Currency.Should().Be("USD");
        item.MasterVariant!.Prices.Should().HaveCount(1);
        item.MasterVariant!.Prices[0].Amount.Should().Be(29.99m);
        item.MasterVariant!.OptionValues.Should().HaveCount(1);
        item.MasterVariant!.OptionValues[0].Name.Should().Be("Red");
        item.MasterVariant!.Images.Should().HaveCount(1);
        item.MasterVariant!.Images[0].Alt.Should().Be("Red shirt");
    }

    [Fact(DisplayName = "Handler: Should map classification with taxon in list response")]
    public async Task Handle_ShouldMapClassificationWithTaxon()
    {
        var product = CreateActiveProduct("Shirt", "shirt");
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories").Value;
        var taxon = TaxonMethod.Create(
            taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts",
            null, null, null, false, null, null, false, null, null).Value;
        taxonomy.Taxons.Add(taxon);
        product.Classifications.Add(ClassificationMethod.Create(product.Id, taxon.Id).Value);

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var item = result.Items.First();
        item.Classifications.Should().HaveCount(1);
        item.Classifications[0].Name.Should().Be("Shirts");
        item.Classifications[0].Slug.Should().Be("shirts");
        item.Classifications[0].Breadcrumb.Should().NotBeEmpty();
        item.Classifications[0].Breadcrumb[0].Name.Should().Be("Shirts");
    }

    [Fact(DisplayName = "Handler: Should exclude discontinued products")]
    public async Task Handle_ShouldExcludeDiscontinuedProducts()
    {
        var product = ProductMethod.Create("Shoes", "shoes", status: ProductStatus.Archived).Value;
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should exclude future products")]
    public async Task Handle_ShouldExcludeFutureProducts()
    {
        var product = CreateActiveProduct("Future Item", "future-item", DateTimeOffset.UtcNow.AddDays(7));
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty when no parameters and no products exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoProducts()
    {
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should filter by option value id and return matching products")]
    public async Task Handle_FiltersByOptionValueId_ReturnsMatchingProducts()
    {
        // Arrange
        var optionType = OptionTypeMethod.Create("Color", "Color", filterable: true).Value;
        var optionValue = OptionValueMethod.Create(optionType.Id, "Red", "Red").Value;
        optionType.OptionValues.Add(optionValue);

        var product = CreateActiveProduct("Red T-Shirt", "red-tshirt");
        var variant = VariantMethod.Create(product.Id, "RED", isMaster: false).Value;
        variant.OptionValueVariants.Add(OptionValueVariantMethod.Create(variant.Id, optionValue.Id).Value);
        product.Variants.Add(variant);

        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters { OptionValueId = [optionValue.Id] };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Red T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should filter by multiple option value ids with OR semantics")]
    public async Task Handle_FiltersByMultipleOptionValueIds_ReturnsAnyMatching()
    {
        // Arrange
        var optionType = OptionTypeMethod.Create("Color", "Color", filterable: true).Value;
        var optionValueRed = OptionValueMethod.Create(optionType.Id, "Red", "Red").Value;
        var optionValueBlue = OptionValueMethod.Create(optionType.Id, "Blue", "Blue").Value;
        optionType.OptionValues.Add(optionValueRed);
        optionType.OptionValues.Add(optionValueBlue);

        var redProduct = CreateActiveProduct("Red Shirt", "red-shirt");
        var redVariant = VariantMethod.Create(redProduct.Id, "RED", isMaster: false).Value;
        redVariant.OptionValueVariants.Add(OptionValueVariantMethod.Create(redVariant.Id, optionValueRed.Id).Value);
        redProduct.Variants.Add(redVariant);

        var blueProduct = CreateActiveProduct("Blue Shirt", "blue-shirt");
        var blueVariant = VariantMethod.Create(blueProduct.Id, "BLU", isMaster: false).Value;
        blueVariant.OptionValueVariants.Add(OptionValueVariantMethod.Create(blueVariant.Id, optionValueBlue.Id).Value);
        blueProduct.Variants.Add(blueVariant);

        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<Product>().Add(redProduct);
        _dbContext.Set<Product>().Add(blueProduct);
        _dbContext.Set<Variant>().Add(redVariant);
        _dbContext.Set<Variant>().Add(blueVariant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters
        {
            OptionValueId = [optionValueRed.Id, optionValueBlue.Id]
        };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert: Both products returned (OR semantics)
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should filter by taxon id and return matching products")]
    public async Task Handle_FiltersByTaxonId_ReturnsMatchingProducts()
    {
        // Arrange
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories").Value;
        var taxon = TaxonMethod.Create(
            taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts",
            null, null, null, false, null, null, false, null, null).Value;
        taxonomy.Taxons.Add(taxon);

        var product = CreateActiveProduct("Casual Shirt", "casual-shirt");
        product.Classifications.Add(ClassificationMethod.Create(product.Id, taxon.Id).Value);

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters { TaxonId = [taxon.Id] };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Casual Shirt");
    }

    [Fact(DisplayName = "Handler: Should filter by multiple taxon ids with OR semantics")]
    public async Task Handle_FiltersByMultipleTaxonIds_ReturnsAnyMatching()
    {
        // Arrange
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories").Value;
        var taxonShirts = TaxonMethod.Create(
            taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts",
            null, null, null, false, null, null, false, null, null).Value;
        var taxonPants = TaxonMethod.Create(
            taxonomy.Id, null, "Pants", "Pants", null, 0, "pants",
            null, null, null, false, null, null, false, null, null).Value;
        taxonomy.Taxons.Add(taxonShirts);
        taxonomy.Taxons.Add(taxonPants);

        var shirtProduct = CreateActiveProduct("Shirt", "shirt");
        shirtProduct.Classifications.Add(ClassificationMethod.Create(shirtProduct.Id, taxonShirts.Id).Value);

        var pantsProduct = CreateActiveProduct("Pants", "pants");
        pantsProduct.Classifications.Add(ClassificationMethod.Create(pantsProduct.Id, taxonPants.Id).Value);

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(shirtProduct);
        _dbContext.Set<Product>().Add(pantsProduct);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters
        {
            TaxonId = [taxonShirts.Id, taxonPants.Id]
        };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert: Both products returned (OR semantics)
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty when option value id does not match")]
    public async Task Handle_ReturnsEmpty_WhenOptionValueIdDoesNotMatch()
    {
        // Arrange
        var optionType = OptionTypeMethod.Create("Color", "Color", filterable: true).Value;
        var optionValue = OptionValueMethod.Create(optionType.Id, "Blue", "Blue").Value;
        optionType.OptionValues.Add(optionValue);

        var product = CreateActiveProduct("Blue T-Shirt", "blue-tshirt");
        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters { OptionValueId = [Guid.NewGuid()] };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should apply price range via min_price/max_price aliases")]
    public async Task Handle_ShouldApplyPriceRangeViaMinMaxPriceAliases()
    {
        var cheapProduct = CreateActiveProduct("Cheap Item", "cheap-item");
        var cheapVariant = VariantMethod.Create(cheapProduct.Id, "M").Value;
        cheapVariant.Prices.Add(PriceMethod.Create(5m, "USD", cheapVariant.Id).Value);
        cheapProduct.Variants.Add(cheapVariant);
        _dbContext.Set<Variant>().Add(cheapVariant);
        _dbContext.Set<Product>().Add(cheapProduct);

        var priceyProduct = CreateActiveProduct("Pricey Item", "pricey-item");
        var priceyVariant = VariantMethod.Create(priceyProduct.Id, "M").Value;
        priceyVariant.Prices.Add(PriceMethod.Create(50m, "USD", priceyVariant.Id).Value);
        priceyProduct.Variants.Add(priceyVariant);
        _dbContext.Set<Variant>().Add(priceyVariant);
        _dbContext.Set<Product>().Add(priceyProduct);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters { MinPrice = 10, MaxPrice = 40 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should filter products within a price range")]
    public async Task Handle_FiltersByPriceRange_ReturnsProductsInPriceRange()
    {
        // Arrange
        var cheapProduct = CreateActiveProduct("Cheap Item", "cheap-item");
        var cheapVariant = VariantMethod.Create(cheapProduct.Id, "M").Value;
        cheapVariant.Prices.Add(PriceMethod.Create(15m, "USD", cheapVariant.Id).Value);
        cheapProduct.Variants.Add(cheapVariant);
        _dbContext.Set<Variant>().Add(cheapVariant);
        _dbContext.Set<Product>().Add(cheapProduct);

        var midProduct = CreateActiveProduct("Mid Item", "mid-item");
        var midVariant = VariantMethod.Create(midProduct.Id, "M").Value;
        midVariant.Prices.Add(PriceMethod.Create(50m, "USD", midVariant.Id).Value);
        midProduct.Variants.Add(midVariant);
        _dbContext.Set<Variant>().Add(midVariant);
        _dbContext.Set<Product>().Add(midProduct);

        var expensiveProduct = CreateActiveProduct("Expensive Item", "expensive-item");
        var expensiveVariant = VariantMethod.Create(expensiveProduct.Id, "M").Value;
        expensiveVariant.Prices.Add(PriceMethod.Create(150m, "USD", expensiveVariant.Id).Value);
        expensiveProduct.Variants.Add(expensiveVariant);
        _dbContext.Set<Variant>().Add(expensiveVariant);
        _dbContext.Set<Product>().Add(expensiveProduct);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetStorefrontProducts.Parameters { MinPrice = 20m, MaxPrice = 100m };

        // Act
        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(parameters),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Mid Item");
    }

    [Fact(DisplayName = "Handler: Should silently ignore unwhitelisted raw filter field")]
    public async Task Handle_ShouldSilentlyIgnoreUnwhitelistedRawFilterField()
    {
        var product = CreateActiveProduct("Unfiltered Product", "unfiltered-product");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters
            {
                Filter = "SomeSecretProperty=value"
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Unfiltered Product");
    }

    [Fact(DisplayName = "Handler: Should return products without facets")]
    public async Task Handle_ShouldReturnProductsWithoutFacets()
    {
        var optionType = OptionTypeMethod.Create("Color", "Color", filterable: true).Value;
        var redValue = OptionValueMethod.Create(optionType.Id, "Red", "Red").Value;
        optionType.OptionValues.Add(redValue);

        var redProduct = CreateActiveProduct("Red Shirt", "red-shirt");
        var redVariant = VariantMethod.Create(redProduct.Id, "RED", isMaster: false).Value;
        redVariant.OptionValueVariants.Add(OptionValueVariantMethod.Create(redVariant.Id, redValue.Id).Value);
        redProduct.Variants.Add(redVariant);

        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<Product>().Add(redProduct);
        _dbContext.Set<Variant>().Add(redVariant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should not return facets by default")]
    public async Task Handle_ShouldNotReturnFacets()
    {
        var product = CreateActiveProduct("Plain Product", "plain-product");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetStorefrontProducts.Query(new GetStorefrontProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.First().Classifications.Should().NotBeNull();
    }
}
