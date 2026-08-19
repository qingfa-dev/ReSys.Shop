using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Storefront.Shared.Mappings;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VisualSearchModelMapping")]
public class VisualSearchModelMappingTests
{
    [Fact(DisplayName = "MapToVisualSearchModel: Should map ModelMetadata to VisualSearchModelResponse")]
    public void MapToVisualSearchModel_ShouldMapAllFields()
    {
        var metadata = new ModelMetadata
        {
            Id = "model-001",
            Name = "Fashion-CLIP",
            Dimension = 512,
            Description = "Fashion CLIP embedding model",
            IsOnnx = true
        };

        var response = metadata.MapToVisualSearchModel<VisualSearchModelResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(metadata.Id);
        response.Name.Should().Be(metadata.Name);
        response.Dimension.Should().Be(metadata.Dimension);
        response.Description.Should().Be(metadata.Description);
        response.IsOnnx.Should().Be(metadata.IsOnnx);
    }

    [Fact(DisplayName = "MapToVisualSearchModel: Should map null Description to null")]
    public void MapToVisualSearchModel_WhenNullDescription_ShouldMapToNull()
    {
        var metadata = new ModelMetadata
        {
            Id = "model-002",
            Name = "Fashion-CLIP",
            Dimension = 512,
            Description = null,
            IsOnnx = false
        };

        var response = metadata.MapToVisualSearchModel<VisualSearchModelResponse>();

        response.Should().NotBeNull();
        response.Description.Should().BeNull();
        response.Id.Should().Be(metadata.Id);
        response.Name.Should().Be(metadata.Name);
        response.Dimension.Should().Be(metadata.Dimension);
        response.IsOnnx.Should().Be(metadata.IsOnnx);
    }
}
