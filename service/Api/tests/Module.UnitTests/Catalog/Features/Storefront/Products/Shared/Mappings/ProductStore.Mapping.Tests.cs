using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Options;
using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductStoreMapping")]
public class ProductStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreDetail: Should map Product to StoreProductDetailResponse")]
    public void MapToStoreDetail_ShouldMapEntityToDetail()
    {
        var product = CreateProduct();

        var response = product.MapToStoreDetail<StoreProductDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(product.Id);
        response.Name.Should().Be(product.Name);
        response.Slug.Should().Be(product.Slug);
        response.Description.Should().Be(product.Description);
        response.MetaTitle.Should().Be(product.MetaTitle);
        response.MetaDescription.Should().Be(product.MetaDescription);
        response.MetaKeywords.Should().Be(product.MetaKeywords);
        response.AvailableOn.Should().Be(product.AvailableOn);
        response.DiscontinueOn.Should().Be(product.DiscontinueOn);
        response.MasterVariantId.Should().Be(product.MasterVariantId);
        response.VariantsCount.Should().Be(2);
        response.MasterVariant.Should().NotBeNull();
        response.MasterVariant!.Id.Should().Be(product.Variants.First(v => v.IsMaster).Id);
        response.Variants.Should().HaveCount(2);
        response.Classifications.Should().HaveCount(1);
    }

    [Fact(DisplayName = "MapToStoreDetail: Should map nested variant option values")]
    public void MapToStoreDetail_ShouldMapVariantOptionValues()
    {
        var product = CreateProductWithOptionValues();

        var response = product.MapToStoreDetail<StoreProductDetailResponse>();

        response.MasterVariant.Should().NotBeNull();
        response.MasterVariant!.OptionValues.Should().HaveCount(1);
        response.MasterVariant!.OptionValues[0].Name.Should().Be("Red");
        response.MasterVariant!.OptionValues[0].OptionTypeName.Should().Be("Color");
    }

    [Fact(DisplayName = "MapToStoreDetail: Should map nested variant prices")]
    public void MapToStoreDetail_ShouldMapVariantPrices()
    {
        var product = CreateProduct();

        var response = product.MapToStoreDetail<StoreProductDetailResponse>();

        response.MasterVariant.Should().NotBeNull();
        response.MasterVariant!.Prices.Should().HaveCount(1);
        response.MasterVariant!.Prices[0].Amount.Should().Be(29.99m);
        response.MasterVariant!.Prices[0].Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "MapToStoreDetail: Should map classification with taxon")]
    public void MapToStoreDetail_ShouldMapClassificationTaxon()
    {
        var product = CreateProduct();

        var response = product.MapToStoreDetail<StoreProductDetailResponse>();

        response.Classifications.Should().HaveCount(1);
        var classification = response.Classifications[0];
        classification.Id.Should().Be(product.Classifications.First().Taxon!.Id);
        classification.Name.Should().Be("clothing");
        classification.Slug.Should().Be("clothing");
    }

    [Fact(DisplayName = "MapToStoreVariant: Should map Variant to StoreProductVariantResponse")]
    public void MapToStoreVariant_ShouldMapVariant()
    {
        var product = CreateProduct();
        var variant = product.Variants.First(v => v.IsMaster);

        var response = variant.MapToStoreVariant<StoreProductVariantResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(variant.Id);
        response.Sku.Should().Be(variant.Sku);
        response.IsMaster.Should().BeTrue();
        response.Price.Should().Be(29.99m);
        response.Currency.Should().Be("USD");
        response.Images.Should().HaveCount(1);
        response.Prices.Should().HaveCount(1);
        response.Prices[0].Amount.Should().Be(29.99m);
        response.Prices[0].Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "MapToStoreVariant: Should map option values from variant")]
    public void MapToStoreVariant_ShouldMapOptionValues()
    {
        var product = CreateProductWithOptionValues();
        var variant = product.Variants.First(v => v.IsMaster);

        var response = variant.MapToStoreVariant<StoreProductVariantResponse>();

        response.OptionValues.Should().HaveCount(1);
        response.OptionValues[0].Name.Should().Be("Red");
        response.OptionValues[0].OptionTypeName.Should().Be("Color");
    }

    [Fact(DisplayName = "MapToStoreImage: Should map VariantImage to StoreProductImageResponse")]
    public void MapToStoreImage_ShouldMapImage()
    {
        var image = CreateImage();

        var response = image.MapToStoreListItem<StoreVariantImageListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(image.Id);
        response.Url.Should().Be("https://example.com/img.jpg");
        response.Alt.Should().Be("Test image");
        response.Position.Should().Be(1);
        response.ContentType.Should().Be("image/jpeg");
    }

    [Fact(DisplayName = "MapToStoreListItem: Should map Product to StoreProductListItemResponse")]
    public void MapToStoreListItem_ShouldMapEntityToList()
    {
        var product = CreateProduct();

        var response = product.MapToStoreListItem<StoreProductListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(product.Id);
        response.Name.Should().Be(product.Name);
        response.Slug.Should().Be(product.Slug);
        response.Description.Should().Be(product.Description);
        response.MasterVariant.Should().NotBeNull();
        response.MasterVariant!.Price.Should().Be(29.99m);
        response.MasterVariant!.Currency.Should().Be("USD");
        response.MasterVariant!.Images.Should().NotBeEmpty();
        response.MasterVariant!.Images[0].Alt.Should().Be("Test image");
        response.MasterVariant!.Prices.Should().HaveCount(1);
        response.MasterVariant!.Prices[0].Amount.Should().Be(29.99m);
        response.AvailableOn.Should().Be(product.AvailableOn);
        response.VariantsCount.Should().Be(2);
        response.Classifications.Should().HaveCount(1);
        response.Classifications[0].Name.Should().Be("clothing");
    }

    [Fact(DisplayName = "MapToStoreListItem: Should handle missing master variant")]
    public void MapToStoreListItem_WhenNoMasterVariant_ShouldHandleGracefully()
    {
        var productResult = ProductMethod.Create(
            "Test Product", "test-product", status: ProductStatus.Active);
        productResult.IsSuccess.Should().BeTrue();
        var product = productResult.Value;

        var response = product.MapToStoreListItem<StoreProductListItemResponse>();

        response.MasterVariant.Should().BeNull();
        response.VariantsCount.Should().Be(0);
    }

    private static Product CreateProduct()
    {
        var productResult = ProductMethod.Create(
            name: "Test Product",
            slug: "test-product",
            description: "Test Description",
            status: ProductStatus.Active,
            availableOn: DateTimeOffset.UtcNow,
            metaTitle: "Meta Title",
            metaDescription: "Meta Desc",
            metaKeywords: "keywords",
            discontinueOn: null, makeActiveAt: null);
        productResult.IsSuccess.Should().BeTrue();
        var product = productResult.Value;

        var variantResult = VariantMethod.Create(
            product.Id, "SKU-001", isMaster: true, position: 1);
        variantResult.IsSuccess.Should().BeTrue();
        var variant = variantResult.Value;
        product.Variants.Add(variant);

        var nonMasterVariantResult = VariantMethod.Create(
            product.Id, "SKU-002", isMaster: false, position: 2);
        nonMasterVariantResult.IsSuccess.Should().BeTrue();
        product.Variants.Add(nonMasterVariantResult.Value);

        var priceResult = PriceMethod.Create(29.99m, "USD", variantId: variant.Id);
        priceResult.IsSuccess.Should().BeTrue();
        variant.Prices.Add(priceResult.Value);

        var image = CreateImage();
        image.VariantId = variant.Id;
        variant.VariantImages.Add(image);

        var taxonResult = TaxonMethod.Create(
            taxonomyId: Guid.NewGuid(), parentId: null, name: "Clothing", presentation: "Clothing",
            description: null, position: 0, slug: "clothing", metaTitle: null, metaDescription: null, metaKeywords: null,
            automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
            imageUrl: null, squareImageUrl: null);
        taxonResult.IsSuccess.Should().BeTrue();
        var taxon = taxonResult.Value;
        var classificationResult = ClassificationMethod.Create(productId: product.Id, taxonId: taxon.Id);
        classificationResult.IsSuccess.Should().BeTrue();
        classificationResult.Value.Taxon = taxon;
        product.Classifications.Add(classificationResult.Value);

        return product;
    }

    private static Product CreateProductWithOptionValues()
    {
        var product = CreateProduct();
        var variant = product.Variants.First(v => v.IsMaster);

        var optionTypeResult = OptionTypeMethod.Create("Color", "Color", filterable: true);
        optionTypeResult.IsSuccess.Should().BeTrue();
        var optionType = optionTypeResult.Value;

        var optionValueResult = OptionValueMethod.Create(optionType.Id, "Red", "Red");
        optionValueResult.IsSuccess.Should().BeTrue();
        var optionValue = optionValueResult.Value;
        optionValue.OptionType = optionType;
        optionType.OptionValues.Add(optionValue);

        var ovdResult = OptionValueVariantMethod.Create(variant.Id, optionValue.Id);
        ovdResult.IsSuccess.Should().BeTrue();
        var ovd = ovdResult.Value;
        ovd.OptionValue = optionValue;
        variant.OptionValueVariants.Add(ovd);

        return product;
    }

    private static VariantImage CreateImage()
    {
        var imageResult = VariantImageMethod.Create(
            "image/jpeg", "img.jpg", 1024,
            "https://example.com/img.jpg", "/storage/img.jpg",
            position: 1, alt: "Test image", type: VariantImageType.Default);
        imageResult.IsSuccess.Should().BeTrue();
        return imageResult.Value;
    }
}
