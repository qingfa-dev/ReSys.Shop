using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Product")]
[Trait("Concern", "Mapping")]
public class ProductMappingTests
{
    [Fact(DisplayName = "MapToDomain: Should map ProductRequest to new Product entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var request = new ProductRequest
        {
            Name = "T-Shirt",
            Slug = "t-shirt",
            Description = "A cotton t-shirt",
            MetaTitle = "Buy T-Shirt",
            MetaDescription = "Premium cotton t-shirt",
            MetaKeywords = "t-shirt, cotton",
            AvailableOn = DateTimeOffset.UtcNow,
            TaxCategoryId = Guid.NewGuid(),
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Slug.Should().Be(request.Slug);
        entity.Description.Should().Be(request.Description);
        entity.MetaTitle.Should().Be(request.MetaTitle);
        entity.MetaDescription.Should().Be(request.MetaDescription);
        entity.MetaKeywords.Should().Be(request.MetaKeywords);
        entity.AvailableOn.Should().Be(request.AvailableOn);
        entity.TaxCategoryId.Should().Be(request.TaxCategoryId);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing Product entity from request")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var request = new ProductRequest
        {
            Name = "New Name",
            Slug = "new-slug",
            Description = "New description",
            MetaTitle = "New Title",
            MetaDescription = "New meta description",
            MetaKeywords = "new, keywords",
            AvailableOn = DateTimeOffset.UtcNow.AddDays(1),
            TaxCategoryId = Guid.NewGuid(),
        };

        var entity = ProductMethod.Create("Old Name", "old-slug", status: ProductStatus.Active).Value;

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be(request.Name);
        entity.Slug.Should().Be(request.Slug);
        entity.Description.Should().Be(request.Description);
        entity.MetaTitle.Should().Be(request.MetaTitle);
        entity.MetaDescription.Should().Be(request.MetaDescription);
        entity.MetaKeywords.Should().Be(request.MetaKeywords);
        entity.AvailableOn.Should().Be(request.AvailableOn);
        entity.TaxCategoryId.Should().Be(request.TaxCategoryId);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should preserve fields when request values are null")]
    public void MapToDomain_Update_ShouldPreserveOtherFields()
    {
        var entity = ProductMethod.Create("Original", "original-slug", description: "Original desc", status: ProductStatus.Active).Value;

        var request = new ProductRequest
        {
            Name = "Updated",
            Slug = entity.Slug,
            Description = entity.Description,
        };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be("Updated");
        entity.Slug.Should().Be("original-slug");
        entity.Description.Should().Be("Original desc");
    }

    [Fact(DisplayName = "MapToDetail: Should map Product entity to ProductDetailResponse")]
    public void MapToDetail_ShouldMapEntityToResponse()
    {
        var entity = ProductMethod.Create("T-Shirt", "t-shirt", description: "A shirt", status: ProductStatus.Active).Value;
        entity.MasterVariantId = Guid.NewGuid();
        entity.CreatedAtUtc = DateTimeOffset.UtcNow;

        var result = entity.MapToDetail<ProductDetailResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Slug.Should().Be(entity.Slug);
        result.Description.Should().Be(entity.Description);
        result.Status.Should().Be(entity.Status);
        result.MasterVariantId.Should().Be(entity.MasterVariantId);
        result.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        result.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map Product entity to ProductListItemResponse")]
    public void MapToListItem_ShouldMapEntityToResponse()
    {
        var entity = ProductMethod.Create("T-Shirt", "t-shirt", status: ProductStatus.Draft).Value;
        entity.MasterVariantId = Guid.NewGuid();

        var result = entity.MapToListItem<ProductListItemResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Slug.Should().Be(entity.Slug);
        result.Status.Should().Be(entity.Status);
        result.MasterVariantId.Should().Be(entity.MasterVariantId);
        result.VariantsCount.Should().Be(0);
    }

    [Fact(DisplayName = "MapToListItem: Should include variants count when entity has children")]
    public void MapToListItem_ShouldIncludeVariantsCount()
    {
        var entity = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;
        var variant = Module.Catalog.Domain.Products.Variants.VariantMethod.Create(entity.Id, "SKU-001", isMaster: true).Value;
        entity.Variants.Add(variant);

        var result = entity.MapToListItem<ProductListItemResponse>();

        result.VariantsCount.Should().Be(1);
    }
}
