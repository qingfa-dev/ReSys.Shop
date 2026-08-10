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
        // EDGE MODELS
        // =========================

        public static readonly ModelSpecification ConvNextTiny =
            new()
            {
                Name = VariantImageConstant.AIModels.ConvNextTiny,
                Dimensions = 384,
                Role = ModelRole.ImageEmbedding,
                ComputeProfile = ComputeProfile.EdgeCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 20,
                UseCase = "Fast embedding generation for filtering and edge pre-processing",
                Strengths = "Very fast CPU inference, stable embeddings",
                Weaknesses = "Limited semantic richness"
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
                UseCase = "Mobile/low-power image embedding",
                Strengths = "Lightweight and stable",
                Weaknesses = "Older architecture, weaker semantic separation"
            };

        // =========================
        // MULTIMODAL (TEXT + IMAGE)
        // =========================

        public static readonly ModelSpecification OpenClipB32 =
            new()
            {
                Name = VariantImageConstant.AIModels.OpenClipB32,
                Dimensions = 512,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 60,
                UseCase = "General text-image retrieval",
                Strengths = "Balanced accuracy and speed",
                Weaknesses = "Weak fashion-specific understanding"
            };

        public static readonly ModelSpecification SigLipBase =
            new()
            {
                Name = VariantImageConstant.AIModels.SigLipBase,
                Dimensions = 768,
                Role = ModelRole.TextImageEmbedding,
                ComputeProfile = ComputeProfile.GpuPreferred,
                SupportsText = true,
                SupportsImage = true,
                ExpectedLatencyMs = 70,
                UseCase = "Improved semantic ranking over CLIP",
                Strengths = "Better alignment quality",
                Weaknesses = "Higher compute cost"
            };

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
                UseCase = "Fashion-aware semantic search",
                Strengths = "Strong style and apparel understanding",
                Weaknesses = "Domain-specific bias"
            };

        // =========================
        // VISUAL SIMILARITY
        // =========================

        public static readonly ModelSpecification DinoV2Small =
            new()
            {
                Name = VariantImageConstant.AIModels.DinoV2Small,
                Dimensions = 384,
                Role = ModelRole.VisualSimilarity,
                ComputeProfile = ComputeProfile.MidCpu,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 80,
                UseCase = "Find visually similar products",
                Strengths = "Excellent fine-grained similarity",
                Weaknesses = "No text understanding"
            };

        public static readonly ModelSpecification SwinBase =
            new()
            {
                Name = VariantImageConstant.AIModels.SwinBase,
                Dimensions = 1024,
                Role = ModelRole.VisualSimilarity,
                ComputeProfile = ComputeProfile.GpuPreferred,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 120,
                UseCase = "Structural and hierarchical image understanding",
                Strengths = "Strong spatial reasoning",
                Weaknesses = "Heavier inference cost"
            };

        public static readonly ModelSpecification Ibot =
            new()
            {
                Name = VariantImageConstant.AIModels.Ibot,
                Dimensions = 768,
                Role = ModelRole.VisualSimilarity,
                ComputeProfile = ComputeProfile.GpuPreferred,
                SupportsText = false,
                SupportsImage = true,
                ExpectedLatencyMs = 110,
                UseCase = "Self-supervised representation learning",
                Strengths = "Strong instance-level similarity",
                Weaknesses = "Complex deployment"
            };

        // =========================
        // REGISTRY
        // =========================

        public static readonly IReadOnlyList<ModelSpecification> All =
            new[]
            {
                ConvNextTiny,
                EfficientNetB0,
                OpenClipB32,
                SigLipBase,
                FashionClip,
                DinoV2Small,
                SwinBase,
                Ibot
            };
    }
}