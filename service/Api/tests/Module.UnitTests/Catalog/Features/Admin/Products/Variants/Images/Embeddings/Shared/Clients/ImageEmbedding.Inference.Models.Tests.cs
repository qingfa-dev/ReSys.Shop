using System.Text.Json;

using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Inference")]
public class InferenceModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact(DisplayName = "EmbeddingRequest: JSON property names use snake_case")]
    public void EmbeddingRequest_SerializesToCamelCase()
    {
        var request = new EmbeddingRequest
        {
            ImageUrl = "http://test/img.jpg",
            ModelName = "efficientnet_b0"
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);

        json.Should().Contain("\"image_url\"");
        json.Should().Contain("\"model_name\"");
        json.Should().NotContain("\"ImageUrl\"");
        json.Should().NotContain("\"Model\"");
    }

    [Fact(DisplayName = "EmbeddingRequest: Deserializes from snake_case JSON")]
    public void EmbeddingRequest_DeserializesFromCamelCase()
    {
        var json = """{"image_url":"http://test/img.jpg","model_name":"efficientnet_b0"}""";

        var request = JsonSerializer.Deserialize<EmbeddingRequest>(json, JsonOptions);

        request.Should().NotBeNull();
        request!.ImageUrl.Should().Be("http://test/img.jpg");
        request.ModelName.Should().Be("efficientnet_b0");
    }

    [Fact(DisplayName = "EmbeddingResponse: JSON property names use snake_case")]
    public void EmbeddingResponse_SerializesToCamelCase()
    {
        var response = new EmbeddingResponse
        {
            Vector = [0.1f, 0.2f],
            ModelVersion = "v1.0",
            Dimension = 2,
            Metadata = new Dictionary<string, object> { ["key"] = "value" }
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);

        json.Should().Contain("\"vector\"");
        json.Should().Contain("\"model_version\"");
        json.Should().Contain("\"dimension\"");
        json.Should().NotContain("\"ModelVersion\"");
    }

    [Fact(DisplayName = "EmbeddingResponse: Deserializes with metadata dictionary")]
    public void EmbeddingResponse_WithMetadata_DeserializesCorrectly()
    {
        var json = """{"vector":[0.1,0.2],"model_version":"v1.0","dimension":2,"metadata":{"model":"efficientnet","accuracy":0.95}}""";

        var response = JsonSerializer.Deserialize<EmbeddingResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Vector.Should().BeEquivalentTo(new List<float> { 0.1f, 0.2f });
        response.ModelVersion.Should().Be("v1.0");
        response.Dimension.Should().Be(2);
        response.Metadata.Should().ContainKey("model");
        response.Metadata.Should().ContainKey("accuracy");
    }

    [Fact(DisplayName = "ModelMetadata: JSON property names use snake_case")]
    public void ModelMetadata_SerializesToCamelCase()
    {
        var metadata = new ModelMetadata
        {
            Id = "efficientnet_b0",
            Name = "EfficientNet B0",
            Dimension = 512,
            Description = "ONNX model",
            IsOnnx = true,
            Tags = ["vision", "classification"]
        };

        var json = JsonSerializer.Serialize(metadata, JsonOptions);

        json.Should().Contain("\"id\"");
        json.Should().Contain("\"name\"");
        json.Should().Contain("\"dimension\"");
        json.Should().Contain("\"is_onnx\"");
        json.Should().NotContain("\"IsOnnx\"");
        json.Should().NotContain("\"Name\"");
    }

    [Fact(DisplayName = "ModelMetadata: Deserializes from snake_case JSON with all fields")]
    public void ModelMetadata_DeserializesFromCamelCase()
    {
        var json = """{"id":"efficientnet_b0","name":"EfficientNet B0","dimension":512,"description":"ONNX model","is_onnx":true,"tags":["vision","classification"]}""";

        var metadata = JsonSerializer.Deserialize<ModelMetadata>(json, JsonOptions);

        metadata.Should().NotBeNull();
        metadata!.Id.Should().Be("efficientnet_b0");
        metadata.Name.Should().Be("EfficientNet B0");
        metadata.Dimension.Should().Be(512);
        metadata.Description.Should().Be("ONNX model");
        metadata.IsOnnx.Should().BeTrue();
        metadata.Tags.Should().BeEquivalentTo(["vision", "classification"]);
    }
}
