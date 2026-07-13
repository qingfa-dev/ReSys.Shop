using System.Text.Json.Serialization;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public class EmbeddingRequest
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = "efficientnet_b0";
}

public class EmbeddingResponse
{
    [JsonPropertyName("vector")]
    public List<float> Vector { get; set; } = [];

    [JsonPropertyName("model_version")]
    public string ModelVersion { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ModelMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_onnx")]
    public bool IsOnnx { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}