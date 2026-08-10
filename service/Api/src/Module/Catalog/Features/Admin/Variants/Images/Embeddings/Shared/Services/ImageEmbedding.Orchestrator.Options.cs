namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

public sealed class EmbeddingOrchestratorOptions
{
    public const string SectionName = "Catalog:EmbeddingOrchestrator";

    public string DefaultModel { get; set; } = Domain.Products.Variants.Images.VariantImageConstant.Defaults.DefaultEmbeddingModel;

    public int TimeoutSeconds { get; set; } = 30;

    public bool RetryOnFailure { get; set; } = true;

    public int MaxRetryAttempts { get; set; } = 3;
}