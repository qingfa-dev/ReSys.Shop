namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

public record EmbeddingDetailResponse
{
    public Guid Id { get; init; }
    public Guid VariantImageId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public float[] Vector { get; init; } = [];
    public int Dimensions { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string Status { get; init; } = "Completed";
    public string? Error { get; init; }
    public string? HangfireJobId { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}