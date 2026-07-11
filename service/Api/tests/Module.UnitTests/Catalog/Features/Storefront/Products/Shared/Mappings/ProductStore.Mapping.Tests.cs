using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.Taxonomies.Taxons;
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
        response.MasterVariant.Should().NotBeNull();
        response.MasterVariant!.Id.Should().Be(product.Variants.First(v => v.IsMaster).Id);
        response.Variants.Should().HaveCount(1); // only non-deleted variants
        response.Taxons.Should().HaveCount(1);
    }

    [Fact(DisplayName = "MapToStoreVariant: Should map Variant to StoreProductVariantResponse")]
    public void MapToStoreVariant_ShouldMapVariant()
    {
        var product = CreateProduct();
        var variant = product.Variants.First(v => v.IsMaster);

        var response = variant.MapToStoreVariant();

        response.Should().NotBeNull();
        response.Id.Should().Be(variant.Id);
        response.Sku.Should().Be(variant.Sku);
        response.IsMaster.Should().BeTrue();
        response.Price.Should().Be(29.99m);
        response.Currency.Should().Be("USD");
        response.Images.Should().HaveCount(1);
    }

    [Fact(DisplayName = "MapToStoreImage: Should map VariantImage to StoreProductImageResponse")]
    public void MapToStoreImage_ShouldMapImage()
    {
        var image = CreateImage();

        var response = image.MapToStoreImage();

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
        response.MinPrice.Should().Be(29.99m);
        response.Currency.Should().Be("USD");
        response.ThumbnailUrl.Should().NotBeNull();
        response.ThumbnailAlt.Should().Be("Test image");
        response.AvailableOn.Should().Be(product.AvailableOn);
        response.VariantsCount.Should().Be(1);
    }

    [Fact(DisplayName = "MapToStoreListItem: Should handle missing master variant")]
    public void MapToStoreListItem_WhenNoMasterVariant_ShouldHandleGracefully()
    {
        var productResult = ProductMethod.Create(
            "Test Product", "test-product", status: ProductStatus.Active);
        productResult.IsSuccess.Should().BeTrue();
        var product = productResult.Value;

        var response = product.MapToStoreListItem<StoreProductListItemResponse>();

        response.MinPrice.Should().BeNull();
        response.Currency.Should().BeNull();
        response.ThumbnailUrl.Should().BeNull();
        response.ThumbnailAlt.Should().BeNull();
        response.VariantsCount.Should().Be(0);
    }

    private static Product CreateProduct()
    {
        var productResult = ProductMethod.Create(
            "Test Product", "test-product", "Test Description",
            ProductStatus.Active, DateTimeOffset.UtcNow,
            "Meta Title", "Meta Desc", "keywords",
            discontinueOn: null, makeActiveAt: null);
        productResult.IsSuccess.Should().BeTrue();
        var product = productResult.Value;

        var variantResult = VariantMethod.Create(
            product.Id, "SKU-001", isMaster: true, position: 1);
        variantResult.IsSuccess.Should().BeTrue();
        var variant = variantResult.Value;
        product.Variants.Add(variant);

        var priceResult = PriceMethod.Create(29.99m, "USD", variantId: variant.Id);
        priceResult.IsSuccess.Should().BeTrue();
        variant.Prices.Add(priceResult.Value);

        var image = CreateImage();
        image.VariantId = variant.Id;
        variant.VariantImages.Add(image);

        var classificationResult = ClassificationMethod.Create(productId: product.Id, taxonId: Guid.NewGuid());
        classificationResult.IsSuccess.Should().BeTrue();
        product.Classifications.Add(classificationResult.Value);

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
