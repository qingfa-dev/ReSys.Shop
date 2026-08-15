using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StoreVariantImageMapping")]
public class StoreVariantImageMappingTests
{
    [Fact(DisplayName = "MapToStoreDownloadItem: Should map VariantImage to StoreVariantImageDownloadResponse including stream")]
    public void MapToStoreDownloadItem_ShouldMapEntityToDownload()
    {
        var id = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var image = new VariantImage
        {
            Id = id,
            VariantId = variantId,
            Url = "https://cdn.test.com/image.webp",
            Alt = "Alt text",
            ContentType = "image/webp",
            FileName = "image.webp",
            FileSize = 5120,
            Width = 800,
            Height = 600,
            StoragePath = "uploads/image.webp",
            Type = VariantImageType.Gallery,
        };

        using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        var response = image.MapToStoreDownloadItem<StoreVariantImageDownloadResponse>(stream);

        response.Should().NotBeNull();
        response.Id.Should().Be(id);
        response.VariantId.Should().Be(variantId);
        response.Url.Should().Be("https://cdn.test.com/image.webp");
        response.Alt.Should().Be("Alt text");
        response.ContentType.Should().Be("image/webp");
        response.FileName.Should().Be("image.webp");
        response.FileSize.Should().Be(5120);
        response.Width.Should().Be(800);
        response.Height.Should().Be(600);
        response.Stream.Should().BeSameAs(stream);
    }
}
