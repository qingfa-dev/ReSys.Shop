
using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantImageMapping")]
public class VariantImageMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map all properties correctly")]
    public void MapToDetail_ShouldMapAllProperties()
    {
        var id = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);

        var entity = new VariantImage
        {
            Id = id,
            VariantId = variantId,
            ContentType = "image/webp",
            FileName = "image.webp",
            FileSize = 5120,
            Width = 800,
            Height = 600,
            DimensionsUnit = "px",
            Position = 2,
            Url = "https://cdn.test.com/image.webp",
            StoragePath = "uploads/image.webp",
            Alt = "Alt text",
            Type = VariantImageType.Gallery,
            CreatedAtUtc = createdAt,
        };

        var result = entity.MapToDetail<VariantImageDetailResponse>();

        result.Id.Should().Be(id);
        result.VariantId.Should().Be(variantId);
        result.Url.Should().Be("https://cdn.test.com/image.webp");
        result.Alt.Should().Be("Alt text");
        result.ContentType.Should().Be("image/webp");
        result.FileName.Should().Be("image.webp");
        result.FileSize.Should().Be(5120);
        result.Width.Should().Be(800);
        result.Height.Should().Be(600);
        result.DimensionsUnit.Should().Be("px");
        result.Position.Should().Be(2);
        result.Type.Should().Be("Gallery");
        result.CreatedAtUtc.Should().Be(createdAt);
    }

    [Fact(DisplayName = "MapToDetail: Should map nullable fields correctly when null")]
    public void MapToDetail_WhenNullableFieldsAreNull_ShouldMapCorrectly()
    {
        var entity = new VariantImage
        {
            Id = Guid.NewGuid(),
            ContentType = "image/jpeg",
            FileName = "img.jpg",
            FileSize = 1024,
            Url = "https://cdn.test.com/img.jpg",
            StoragePath = "uploads/img.jpg",
            Type = VariantImageType.Default,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var result = entity.MapToDetail<VariantImageDetailResponse>();

        result.VariantId.Should().BeNull();
        result.Alt.Should().BeNull();
        result.Width.Should().BeNull();
        result.Height.Should().BeNull();
        result.DimensionsUnit.Should().BeNull();
    }

    [Fact(DisplayName = "MapToDetail: Should map to derived response type")]
    public void MapToDetail_ShouldMapToDerivedType()
    {
        var entity = new VariantImage
        {
            Id = Guid.NewGuid(),
            ContentType = "image/jpeg",
            FileName = "img.jpg",
            FileSize = 1024,
            Url = "https://cdn.test.com/img.jpg",
            StoragePath = "uploads/img.jpg",
            Type = VariantImageType.Thumbnail,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var result = entity.MapToDetail<TestDetailResponse>();

        result.Should().BeOfType<TestDetailResponse>();
        result.FileName.Should().Be("img.jpg");
    }

    private sealed record TestDetailResponse : VariantImageDetailResponse
    {
    }

    [Fact(DisplayName = "MapToDownload: Should map all fields and include stream")]
    public void MapToDownload_ShouldMapWithStream()
    {
        var entity = new VariantImage
        {
            Id = Guid.NewGuid(),
            ContentType = "image/png",
            FileName = "download.png",
            FileSize = 4096,
            Url = "https://cdn.test.com/download.png",
            StoragePath = "uploads/download.png",
            Type = VariantImageType.Default,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        var result = entity.MapToDownload<TestDownloadResponse>(stream);

        result.Should().BeOfType<TestDownloadResponse>();
        result.FileName.Should().Be("download.png");
        result.ContentType.Should().Be("image/png");
        result.Stream.Should().BeSameAs(stream);
    }

    private sealed record TestDownloadResponse : VariantImageDownloadResponse
    {
    }
}
