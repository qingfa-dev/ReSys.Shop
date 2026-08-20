using System.Text.Json.Serialization;
using Module.Catalog.Domain.Variants.Images;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;

public class EmbeddingRequest
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = VariantImageConstant.Defaults.DefaultEmbeddingModel;
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