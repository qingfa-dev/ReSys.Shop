using Module.Catalog.Domain.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record ImageEmbeddingParameters
{
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
}

public record CreateEmbeddingRequest : ImageEmbeddingParameters;

public record RegenerateEmbeddingRequest : ImageEmbeddingParameters;

public record EmbeddingDetailResponse
{
    public Guid Id { get; init; }
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public float[] Vector { get; init; } = [];
    public int Dimensions { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public EmbeddingStatus Status { get; init; } = EmbeddingStatus.Completed;
    public string? Error { get; init; }
    public string? HangfireJobId { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
