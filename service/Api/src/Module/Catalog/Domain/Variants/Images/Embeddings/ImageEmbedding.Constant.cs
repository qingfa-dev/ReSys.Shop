namespace Module.Catalog.Domain.Variants.Images.Embeddings;

public enum EmbeddingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public static class ImageEmbeddingConstant
{
    public static class Constraints
    {
        public const int DefaultVectorDimensions = 512; // CLIP
        public const int ModelNameMaxLength = 100;
        public const int ModelVersionMaxLength = 50;
    }

    public enum ModelRole
    {
        ImageEmbedding,
        TextImageEmbedding,
        VisualSimilarity,
        AttributeExtraction,
        Reranker
    }

    public enum ComputeProfile
    {
        EdgeCpu,
        MidCpu,
        GpuPreferred,
        GpuRequired
    }

    public record ModelSpecification
    {
        public string Name { get; init; } = default!;
        public int Dimensions { get; init; }
        public ModelRole Role { get; init; }
        public ComputeProfile ComputeProfile { get; init; }
        public bool SupportsText { get; init; }
        public bool SupportsImage { get; init; }
        public int ExpectedLatencyMs { get; init; }
        public string UseCase { get; init; } = default!;
        public string Strengths { get; init; } = default!;
        public string Weaknesses { get; init; } = default!;
    }

    public static class Models
    {
        // =========================
        // CNN
        // =========================

        public static readonly ModelSpecification ConvNextTiny =
            new()
            {
                Name = VariantImageConstant.AIModels.ConvNextTiny,
                Dimensions = 768,
                Role = ModelRole.ImageEmbedding,
                ComputeProfile = ComputeProfile.EdgeCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 20,
                UseCase = "Fast visual embedding generation",
                Strengths = "Efficient inference with strong visual features",
                Weaknesses = "Limited semantic richness compared with vision-language models"
            };

        public static readonly ModelSpecification EfficientNetB0 =
            new()
            {
                Name = VariantImageConstant.AIModels.EfficientNetB0,
                Dimensions = 1280,
                Role = ModelRole.ImageEmbedding,
                ComputeProfile = ComputeProfile.EdgeCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 25,
                UseCase = "Lightweight visual embedding generation",
                Strengths = "Lightweight and stable",
                Weaknesses = "Weaker semantic representation than vision-language models"
            };

        public static readonly ModelSpecification ResNet50 =
            new()
            {
                Name = VariantImageConstant.AIModels.ResNet50,
                Dimensions = 2048,
                Role = ModelRole.ImageEmbedding,
                ComputeProfile = ComputeProfile.EdgeCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 30,
                UseCase = "Baseline CNN embedding for benchmark and ablation studies",
                Strengths = "Well-understood and stable features",
                Weaknesses = "High dimensionality and no text-image alignment"
            };

        // =========================
        // CLIP
        // =========================

        public static readonly ModelSpecification ClipB32 =
            new()
            {
                Name = VariantImageConstant.AIModels.ClipB32,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 60,
                UseCase = "General text-image retrieval",
                Strengths = "Balanced representation quality and inference cost",
                Weaknesses = "Limited fashion-specific understanding"
            };

        public static readonly ModelSpecification ClipL14 =
            new()
            {
                Name = VariantImageConstant.AIModels.ClipL14,
                Dimensions = 768,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 80,
                UseCase = "Higher-capacity text-image retrieval",
                Strengths = "Higher-capacity visual-text representation",
                Weaknesses = "Higher computational cost"
            };

        public static readonly ModelSpecification ClipViTB16 =
            new()
            {
                Name = VariantImageConstant.AIModels.ClipViTB16,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 65,
                UseCase = "CLIP retrieval with ViT-B/16 image encoder",
                Strengths = "Strong visual representation with 512-dimensional projected embedding",
                Weaknesses = "Higher inference cost than smaller configurations"
            };

        public static readonly ModelSpecification ClipGeneric =
            new()
            {
                Name = VariantImageConstant.AIModels.ClipGeneric,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 60,
                UseCase = "General-purpose CLIP baseline",
                Strengths = "Widely available and easy to deploy",
                Weaknesses = "No fashion-specific domain adaptation"
            };

        public static readonly ModelSpecification SigLIP =
            new()
            {
                Name = VariantImageConstant.AIModels.SigLIP,
                Dimensions = 768,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.GpuPreferred,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 70,
                UseCase = "Semantic image-text retrieval",
                Strengths = "Strong image-text alignment",
                Weaknesses = "Higher computational cost"
            };

        public static readonly ModelSpecification EvaClip =
            new()
            {
                Name = VariantImageConstant.AIModels.EvaClip,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 70,
                UseCase = "Enhanced visual representation",
                Strengths = "Strong visual representation",
                Weaknesses = "Higher model complexity"
            };

        // =========================
        // FASHION-SPECIFIC
        // =========================

        public static readonly ModelSpecification FashionClip =
            new()
            {
                Name = VariantImageConstant.AIModels.FashionClip,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 90,
                UseCase = "Fashion-aware semantic retrieval",
                Strengths = "Strong fashion and apparel representation",
                Weaknesses = "Domain-specific representation"
            };

        // =========================
        // VISUAL SIMILARITY
        // =========================

        public static readonly ModelSpecification DinoV2ViTS14 =
            new()
            {
                Name = VariantImageConstant.AIModels.DinoV2ViTS14,
                Dimensions = 384,
                Role = ModelRole.VisualSimilarity,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 80,
                UseCase = "Fine-grained visual similarity retrieval",
                Strengths = "Strong visual and structural similarity representation",
                Weaknesses = "No text-image alignment"
            };

        public static readonly IReadOnlyList<ModelSpecification> All =
            new[]
            {
            ConvNextTiny,
            EfficientNetB0,
            ResNet50,
            ClipB32,
            ClipL14,
            ClipViTB16,
            ClipGeneric,
            SigLIP,
            EvaClip,
            FashionClip,
            DinoV2ViTS14
            };
    }

    /// <summary>
    /// Maps benchmark model slugs to their embedding vector dimensions.
    /// Used for creating per-model HNSW partial indexes on the untyped vector column.
    /// Keys reference <see cref="VariantImageConstant.AIModels"/> constants.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> VectorDimensions =
        Models.All.ToDictionary(
            model => model.Name,
            model => model.Dimensions,
            StringComparer.OrdinalIgnoreCase);
}